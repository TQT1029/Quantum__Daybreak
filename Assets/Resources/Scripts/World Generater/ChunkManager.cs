using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Trái tim của hệ thống. Quyết định chunk nào được tải, tắt, hoặc hủy
/// dựa trên vị trí của đối tượng tracking (Player).
/// Tối ưu hóa bằng đa luồng (Async/Await Task) để tránh giật lag.
/// </summary>
[RequireComponent(typeof(WorldGenerator))]
public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Loading Logic")]
    [Tooltip("Bán kính chunk được BẬT (Active). Vùng 1.")]
    [SerializeField] private int viewDistance = 2;
    [Tooltip("Bán kính chunk bị HỦY (Unload). PHẢI LỚN HƠN View Distance. Vùng 3.")]
    [SerializeField] private int unloadDistance = 4;

    // Kích thước chunk, lấy từ WorldGenerator
    private int chunkSize;

    /// <summary>
    /// Dictionary chứa tất cả các chunk đang được quản lý (cả bật và tắt).
    /// </summary>
    private static Dictionary<Vector2, ChunkGenerator> chunkDictionary = new Dictionary<Vector2, ChunkGenerator>();
    public static Dictionary<Vector2, ChunkGenerator> ChunkDictionary => chunkDictionary;

    // Tham chiếu đến script chứa cài đặt
    private WorldGenerator worldGenerator;
    // Tọa độ chunk cuối cùng của player, dùng để kiểm tra di chuyển
    private Vector2 lastPlayerChunkCoord = new Vector2(float.MinValue, float.MinValue);
    // List tạm để chứa các chunk cần hủy (tránh sửa Dictionary khi đang duyệt)
    private List<Vector2> chunksToDestroy = new List<Vector2>();
    // HashSet theo dõi các chunk đang được tính toán ở luồng nền
    private HashSet<Vector2> chunksBeingGenerated = new HashSet<Vector2>();

    //Object chứa tất cả chunk để Scene gọn gàng
    private Transform chunkParent;

    private void Start()
    {
        // Tự động lấy reference từ component bên cạnh
        worldGenerator = GetComponent<WorldGenerator>();

        // Đăng ký bản thân với ReferenceManager (để Global truy cập nếu cần)
        if (ReferenceManager.Instance != null)
        {
            ReferenceManager.Instance.ActiveWorld = worldGenerator;
        }

        chunkSize = worldGenerator.ChunkSize;

        // Tự tạo folder chứa chunk gọn gàng
        if (chunkParent == null)
        {
            GameObject gridObj = new GameObject("World_Chunks");
            gridObj.AddComponent<Grid>(); // Bắt buộc cho Tilemap
            chunkParent = gridObj.transform;
        }
    }
    private void Update()
    {
        Transform player = ReferenceManager.Instance.PlayerTransform;

        // Nếu chưa tìm thấy Player hoặc Player chết -> Không update map
        if (player == null) return;

        // Tính toán tọa độ chunk hiện tại của player
        int currentChunkX = Mathf.FloorToInt(player.position.x / chunkSize);
        int currentChunkY = Mathf.FloorToInt(player.position.y / chunkSize);
        Vector2 playerChunkCoord = new Vector2(currentChunkX, currentChunkY);

        // Chỉ update nếu player đã di chuyển sang 1 chunk mới
        if (playerChunkCoord == lastPlayerChunkCoord) return;

        lastPlayerChunkCoord = playerChunkCoord;

        // Gọi hàm async (fire and forget - nó sẽ tự chạy)
        UpdateVisibleChunks(playerChunkCoord);
    }

    /// <summary>
    /// Logic "Hybrid" (Kết hợp) để quản lý chunk (Tải/Tắt/Hủy).
    /// </summary>
    /// <param name="playerChunkCoord">Tọa độ chunk player vừa bước vào</param>
    private void UpdateVisibleChunks(Vector2 playerChunkCoord)
    {
        int currentChunkX = (int)playerChunkCoord.x;
        int currentChunkY = (int)playerChunkCoord.y;

        chunksToDestroy.Clear();

        // --- VÒNG LẶP 1 & 2 (Destroy/Disable/Enable) ---
        // Duyệt qua các chunk đã có để quyết định Tắt, Bật, hoặc Hủy
        // (Chạy trên luồng chính, nhưng rất nhanh)
        foreach (var chunk in chunkDictionary)
        {
            Vector2 chunkCoord = chunk.Key;
            ChunkGenerator chunkInstance = chunk.Value;
            int dist = Mathf.Max(Mathf.Abs((int)chunkCoord.x - currentChunkX), Mathf.Abs((int)chunkCoord.y - currentChunkY));

            if (dist > unloadDistance)
            {
                chunksToDestroy.Add(chunkCoord); // Vùng 3: Hủy
            }
            else if (dist > viewDistance)
            {
                chunkInstance.Disable(); // Vùng 2: Tắt (Cache)
            }
            else
            {
                chunkInstance.Enable(); // Vùng 1: Bật
            }
        }

        // Thực hiện hủy
        foreach (Vector2 chunkCoord in chunksToDestroy)
        {
            chunkDictionary[chunkCoord].DestroyChunk();
            chunkDictionary.Remove(chunkCoord);
        }

        // --- VÒNG LẶP 3: TẠO CHUNK MỚI (Async) ---
        // Duyệt qua các chunk trong tầm nhìn (Vùng 1)
        for (int yOffset = -viewDistance; yOffset <= viewDistance; yOffset++)
        {
            for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
            {
                Vector2 chunkCoord = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);

                // Nếu chunk CHƯA TỒN TẠI và cũng KHÔNG ĐANG ĐƯỢC TẠO
                if (!chunkDictionary.ContainsKey(chunkCoord) && !chunksBeingGenerated.Contains(chunkCoord))
                {
                    // Bắt đầu tác vụ tạo chunk mới (sẽ chạy đa luồng)
                    StartGeneratingChunk(chunkCoord);
                }
            }
        }
    }

    /// <summary>
    /// Bắt đầu một tác vụ đa luồng để tính toán noise và tạo chunk.
    /// </summary>
    /// <param name="chunkCoord">Tọa độ chunk cần tạo</param>
    private async void StartGeneratingChunk(Vector2 chunkCoord)
    {
        // 1. Đánh dấu là "đang tạo" (trên luồng chính)
        chunksBeingGenerated.Add(chunkCoord);

        // 2. "Chụp" (Capture) tất cả các biến cần thiết cho luồng nền
        int localChunkSize = chunkSize;
        int seed = worldGenerator.Seed;

        var heightNoiseParams = worldGenerator.HeightNoiseParams;
        var tempNoiseParams = worldGenerator.TemperatureNoiseParams;
        var moistNoiseParams = worldGenerator.HumidityNoiseParams;

        var heightOffset = worldGenerator.HeightOffset + (chunkCoord * localChunkSize);
        var tempOffset = worldGenerator.TemperatureOffset + (chunkCoord * localChunkSize);
        var moistOffset = worldGenerator.HumidityOffset + (chunkCoord * localChunkSize);

        // 3. Bắt đầu Tác vụ Nền (Background Thread)
        ChunkGenerator.ChunkData noiseData = await Task.Run(() =>
        {
            // === Code này chạy trên LUỒNG NỀN ===
            // Tính 3 map noise. Có thể chạy song song nếu muốn.
            float[,] hMap = NoiseGeneration.GenerateNoiseMap(localChunkSize, seed, heightNoiseParams, heightOffset);
            float[,] tMap = NoiseGeneration.GenerateNoiseMap(localChunkSize, seed + 1, tempNoiseParams, tempOffset);
            float[,] mMap = NoiseGeneration.GenerateNoiseMap(localChunkSize, seed + 2, moistNoiseParams, moistOffset);

            return new ChunkGenerator.ChunkData(hMap, tMap, mMap);
        });

        // 4. === Code này quay lại LUỒNG CHÍNH ===
        chunksBeingGenerated.Remove(chunkCoord);

        // 5. Kiểm tra an toàn: Lỡ người chơi di chuyển quá xa trong lúc chờ?
        if (ReferenceManager.Instance.PlayerTransform == null) return;

        Vector2 playerPos = lastPlayerChunkCoord; // Dùng cached pos
        int dist = Mathf.Max(Mathf.Abs((int)chunkCoord.x - (int)playerPos.x), Mathf.Abs((int)chunkCoord.y - (int)playerPos.y));

        // Chỉ tạo chunk nếu nó vẫn còn trong phạm vi 'unloadDistance'
        if (dist <= unloadDistance && !chunkDictionary.ContainsKey(chunkCoord))
        {
            // 6. Tạo chunk (Tác vụ Unity - Rất nhanh)
            ChunkGenerator newChunk = new ChunkGenerator(chunkCoord, localChunkSize, worldGenerator, chunkParent, noiseData);
            chunkDictionary.Add(chunkCoord, newChunk);

            // Tắt nó đi nếu nó nằm trong vùng "cache" (ngoài view)
            if (dist > viewDistance) newChunk.Disable();
        }
    }
}