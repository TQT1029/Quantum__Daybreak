using UnityEngine;
using UnityEngine.Tilemaps; // Bạn đã dùng RuleTile, nên tôi sẽ dùng TileBase để tổng quát

/// <summary>
/// Lớp chứa DỮ LIỆU CÀI ĐẶT cho thế giới (Seed, Kích thước, Cài đặt Noise).
/// Quyết định Biome nào sẽ xuất hiện dựa trên Nhiệt độ và Độ ẩm.
/// Cũng dùng để chạy preview trong Editor.
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    #region Enums & Structs

    /// <summary>
    /// Các chế độ xem trước (preview) trong Editor.
    /// </summary>
    public enum DrawMode { HeightMap, TempMap, HumidityMap, BiomeMap }

    /// <summary>
    /// Định nghĩa một loại địa hình (ví dụ: Nước, Cát, Cỏ) và tile tương ứng.
    /// </summary>
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        [Tooltip("Giá trị noise TỐI ĐA cho loại địa hình này (chuẩn hóa 0-1)")]
        public float height;
        [Tooltip("Màu sắc dùng cho Editor Preview")]
        public Color color;
        [Tooltip("Tile thật sẽ được đặt trong game")]
        public TileBase tile; // Dùng TileBase (cha của RuleTile) để linh hoạt

    }

    /// <summary>
    /// Gói các tham số noise lại cho dễ quản lý và truyền đi.
    /// </summary>
    [System.Serializable]
    public struct noiseParams
    {
        public float scale;
        public int octaves;
        public float persistence;
        public float lacunarity;
        public float amplitudeFactor;

        public noiseParams(float scale, int octaves, float persistence, float lacunarity, float amplitudeFactor)
        {
            this.scale = scale;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.amplitudeFactor = amplitudeFactor;
        }
    }

    /// <summary>
    /// Định nghĩa các ngưỡng để chọn Biome.
    /// </summary>
    [System.Serializable]
    public struct BiomeSetting
    {
        public string name;
        public Biome biome;

        [Header("Condition, Values in (0-1)")]
        public float minHeight => biome.minHeight;// Chiều cao là điều kiện BẮT BUỘC, bình thường sẽ bằng độ cao cao nhất của biome level trước
        public float maxHeight => biome.maxHeight;//là chiều cao cao nhất của biome này

        public bool useTemperature; // Nếu true, sẽ xét cả nhiệt độ
        public float minTemperature;
        public float maxTemperature;

        public bool useHumidity; // Nếu true, sẽ xét cả độ ẩm
        public float minHumidity;
        public float maxHumidity;
    }

    #endregion

    // --- Biến Inspector ---

    [Header("World Settings")]
    [SerializeField] private int chunkSize = 64;
    [SerializeField] private int seed;

    [Header("Biome Settings")]
    [Tooltip("Danh sách các Biome và điều kiện để chúng xuất hiện")]
    [SerializeField] private BiomeSetting[] biomeSettings;
    [Tooltip("Biome mặc định nếu không thỏa mãn điều kiện nào")]
    [SerializeField] private Biome defaultBiome;

    [Header("Height Noise (Địa hình)")]
    [SerializeField] private noiseParams H_NoiseParams = new noiseParams(150f, 5, 0.5f, 2.2f, 1.25f);
    [SerializeField] private Vector2 H_offset = Vector2.zero;

    [Header("Temperature Noise (Nhiệt độ)")]
    [SerializeField] private noiseParams T_NoiseParams = new noiseParams(50f, 4, 0.5f, 2.0f, 1.0f);
    [SerializeField] private Vector2 T_offset = Vector2.zero;

    [Header("Humidity Noise (Độ ẩm)")]
    [SerializeField] private noiseParams M_NoiseParams = new noiseParams(75f, 4, 0.5f, 2.0f, 1.0f);
    [SerializeField] private Vector2 M_offset = Vector2.zero;

    [Header("Editor Preview")]
    [SerializeField] private DrawMode drawMode;
    [Tooltip("Tự động vẽ lại preview khi thay đổi thông số")]
    public bool autoUpdate = false;

    // --- Properties (để các script khác truy cập) ---
    public int ChunkSize => chunkSize;
    public int Seed => seed;

    // Properties trả về các cài đặt noise
    public noiseParams HeightNoiseParams => H_NoiseParams;
    public noiseParams TemperatureNoiseParams => T_NoiseParams;
    public noiseParams HumidityNoiseParams => M_NoiseParams;

    // Properties trả về các offset
    public Vector2 HeightOffset => H_offset;
    public Vector2 TemperatureOffset => T_offset;
    public Vector2 HumidityOffset => M_offset;

    #region Biome Logic

    /// <summary>
    /// Quyết định Biome dựa trên giá trị Chiều cao, Nhiệt độ và Độ ẩm.
    /// << REFACTOR: Sử dụng logic "best score" (khớp nhất) để tránh chồng lấn >>
    /// </summary>
    /// <param name="height">Giá trị chiều cao (0-1)</param>
    /// <param name="temperature">Giá trị nhiệt độ (0-1)</param>
    /// <param name="humidity">Giá trị độ ẩm (0-1)</param>
    /// <returns>Đối tượng ScriptableObject Biome phù hợp nhất</returns>
    public Biome GetBiome(float height, float temperature, float humidity)
    {
        Biome bestMatchBiome = defaultBiome;
        int bestMatchScore = -1; // Biome mặc định có điểm -1 (thấp nhất)

        foreach (var setting in biomeSettings)
        {
            // Kiểm tra an toàn
            if (setting.biome == null) continue;

            // --- 1. Kiểm tra Chiều cao (bắt buộc) ---
            // Dùng > maxHeight thay vì < maxHeight để fix logic (maxHeight là mốc cuối cùng)
            if (height < setting.minHeight || height > setting.maxHeight)
            {
                continue; // Không nằm trong dải chiều cao, bỏ qua biome này
            }

            int currentScore = 1; // 1 điểm vì đã khớp chiều cao

            // --- 2. Kiểm tra Nhiệt độ (nếu biome này yêu cầu) ---
            if (setting.useTemperature)
            {
                if (temperature >= setting.minTemperature && temperature < setting.maxTemperature)
                {
                    currentScore++; // Khớp, cộng điểm
                }
                else
                {
                    continue; // Yêu cầu nhưng không khớp, bỏ qua biome này
                }
            }

            // --- 3. Kiểm tra Độ ẩm (nếu biome này yêu cầu) ---
            if (setting.useHumidity)
            {
                if (humidity >= setting.minHumidity && humidity < setting.maxHumidity)
                {
                    currentScore++; // Khớp, cộng điểm
                }
                else
                {
                    continue; // Yêu cầu nhưng không khớp, bỏ qua biome này
                }
            }

            // --- 4. So sánh điểm ---
            // Nếu biome này là một lựa chọn hợp lệ VÀ có điểm cao hơn (khớp nhiều
            // điều kiện hơn) biome tốt nhất trước đó, chọn nó.
            if (currentScore > bestMatchScore)
            {
                bestMatchScore = currentScore;
                bestMatchBiome = setting.biome;
            }
        }

        return bestMatchBiome;
    }

    #endregion
    #region Editor Preview Logic

    /// <summary>
    /// (Chỉ dùng cho Editor) Gọi bởi Button hoặc AutoUpdate.
    /// </summary>
    public void GenerateMap()
    {
        // Tạo 3 bản đồ noise MẪU (chạy trên luồng chính, chỉ dùng cho Editor)
        // Chú ý: seed + 1, seed + 2 để đảm bảo 3 map khác nhau
        float[,] hMap = NoiseGeneration.GenerateNoiseMap(chunkSize, seed, HeightNoiseParams, H_offset);
        float[,] tMap = NoiseGeneration.GenerateNoiseMap(chunkSize, seed + 5, TemperatureNoiseParams, T_offset);
        float[,] mMap = NoiseGeneration.GenerateNoiseMap(chunkSize, seed + 2, HumidityNoiseParams, M_offset);

        ChunkGenerator.ChunkData noiseData = new ChunkGenerator.ChunkData(hMap, tMap, mMap);

        // Hiển thị nó lên Sprite Preview
        DisplayMap(noiseData, BuildColorMap(noiseData));
    }

    /// <summary>
    /// (Chỉ dùng cho Editor) Gửi dữ liệu map đến MapDisplay.
    /// </summary>
    private void DisplayMap(ChunkGenerator.ChunkData noiseData, Color[] colorMap)
    {
        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        if (display == null)
        {
            Debug.LogWarning("Không tìm thấy [MapDisplay] trong Scene để vẽ preview.");
            return;
        }

        switch (drawMode)
        {
            case DrawMode.HeightMap:
                display.HeightMap(noiseData.H_noiseMap);
                break;
            case DrawMode.TempMap:
                display.TempMap(noiseData.T_noiseMap);
                break;
            case DrawMode.HumidityMap:
                display.HumidityMap(noiseData.M_noiseMap);
                break;
            case DrawMode.BiomeMap:
                display.DrawColorMap(colorMap, chunkSize);
                break;
        }
    }

    /// <summary>
    /// (Chỉ dùng cho Editor) Chuyển 3 map noise thành 1 bản đồ màu Biome.
    /// << SỬA LỖI: Dùng logic GetBiome để tô màu >>
    /// </summary>
    public Color[] BuildColorMap(ChunkGenerator.ChunkData noiseData)
    {
        int width = noiseData.H_noiseMap.GetLength(0);
        int height = noiseData.H_noiseMap.GetLength(1);
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = noiseData.H_noiseMap[x, y];
                float t = noiseData.T_noiseMap[x, y];
                float m = noiseData.M_noiseMap[x, y];

                // 1. Lấy Biome dựa trên
                Biome biome = GetBiome(h, t, m);
                if (biome == null || biome.terrainTypes == null)
                {
                    colorMap[y * width + x] = Color.magenta; // Lỗi: Biome chưa được gán
                    continue;
                }

                // 2. Dùng Chiều cao (h) để tìm MÀU từ Biome đó
                Color color = Color.black;
                for (int i = 0; i < biome.terrainTypes.Length; i++)
                {
                    if (h <= biome.terrainTypes[i].height)
                    {
                        color = biome.terrainTypes[i].color;
                        break;
                    }
                }
                colorMap[y * width + x] = color;
            }
        }
        return colorMap;
    }

    #endregion

#if UNITY_EDITOR
    /// <summary>
    /// (Chỉ dùng cho Editor) Đảm bảo các giá trị không bị nhập sai.
    /// </summary>
    private void OnValidate()
    {
        if (chunkSize < 1) chunkSize = 1;

        // Helper function để giới hạn noise params
        void ClampNoiseParams(ref noiseParams p)
        {
            if (p.persistence < 0f) p.persistence = 0f; else if (p.persistence > 1f) p.persistence = 1f;
            if (p.scale < 1.0001f) p.scale = 1.0001f;
            if (p.octaves < 1) p.octaves = 1; else if (p.octaves > 12) p.octaves = 12;
            if (p.lacunarity < 1) p.lacunarity = 1;
            if (p.amplitudeFactor < 0.0001f) p.amplitudeFactor = 0.0001f;
        }

        ClampNoiseParams(ref H_NoiseParams);
        ClampNoiseParams(ref T_NoiseParams);
        ClampNoiseParams(ref M_NoiseParams);
    }
#endif
}