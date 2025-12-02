using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Đại diện cho MỘT chunk trong thế giới.
/// Lớp này nhận dữ liệu noise đã được tính toán (từ luồng nền)
/// và chỉ thực hiện các tác vụ của Unity (tạo GameObject, đặt tile).
/// </summary>
public class ChunkGenerator
{
    /// <summary>
    /// Lớp chứa dữ liệu 3 map noise cho chunk, 
    /// được tính toán ở luồng nền và truyền vào constructor.
    /// </summary>
    public class ChunkData
    {
        public readonly float[,] H_noiseMap, T_noiseMap, M_noiseMap;

        public ChunkData(float[,] h, float[,] t, float[,] m)
        {
            this.H_noiseMap = h;
            this.T_noiseMap = t;
            this.M_noiseMap = m;
        }
    }

    private GameObject chunkObject;
    private Tilemap tilemap;
    private bool isVisible;

    /// <summary>
    /// Constructor của Chunk.
    /// </summary>
    /// <param name="chunkCoord">Tọa độ của chunk (ví dụ: (0,0), (1,0))</param>
    /// <param name="size">Kích thước của chunk (ví dụ: 64)</param>
    /// <param name="worldGenerator">Tham chiếu đến WorldGenerator để lấy cài đặt Biome</param>
    /// <param name="parent">Parent Transform để chứa GameObject của chunk</param>
    /// <param name="noiseData">Dữ liệu 3 map noise đã được tính toán</param>
    public ChunkGenerator(Vector2 chunkCoord, int size, WorldGenerator worldGenerator, Transform parent, ChunkData noiseData)
    {
        this.tilemap = CreateTilemapGameObject(chunkCoord, parent);

        // << SỬA: Truyền 'worldGenerator' vào hàm đặt tile >>
        PlaceTilesOptimized(noiseData, worldGenerator, chunkCoord, size);

        isVisible = true;
    }

    /// <summary>
    /// Tạo GameObject vật lý chứa Tilemap cho chunk này.
    /// </summary>
    private Tilemap CreateTilemapGameObject(Vector2 chunkCoord, Transform parent)
    {
        chunkObject = new GameObject($"Chunk {chunkCoord}");
        chunkObject.transform.position = Vector3.zero;
        chunkObject.transform.parent = parent;

        Tilemap tileMap = chunkObject.AddComponent<Tilemap>();
        chunkObject.AddComponent<TilemapRenderer>();

        return tileMap;
    }

    #region Tile Placement

    /// <summary>
    /// Hàm Đặt Tiles (đã tối ưu).
    /// Sử dụng 3 map noise để quyết định Biome, sau đó dùng map Chiều cao để đặt tile.
    /// </summary>
    /// <param name="noiseData">Object chứa 3 map noise</param>
    /// <param name="worldGenerator">Tham chiếu để gọi GetBiome()</param>
    /// <param name="chunkCoord">Tọa độ của chunk</param>
    /// <param name="chunkSize">Kích thước chunk</param>
    private void PlaceTilesOptimized(ChunkData noiseData, WorldGenerator worldGenerator, Vector2 chunkCoord, int chunkSize)
    {
        int chunkWorldX = (int)chunkCoord.x * chunkSize;
        int chunkWorldY = (int)chunkCoord.y * chunkSize;

        int totalTiles = chunkSize * chunkSize;

        Vector3Int[] positions = new Vector3Int[totalTiles];
        TileBase[] tiles = new TileBase[totalTiles];

        int index = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                // 1. Lấy 3 giá trị noise
                float h = noiseData.H_noiseMap[x, y];
                float t = noiseData.T_noiseMap[x, y];
                float m = noiseData.M_noiseMap[x, y];

                // 2. Quyết định Biome
                Biome biome = worldGenerator.GetBiome(h, t, m);

                // 3. Tìm tile dựa trên Chiều cao (h) và danh sách tile CỦA BIOME ĐÓ
                TileBase tileToPlace = null;
                if (biome != null) // Kiểm tra an toàn
                {
                    tileToPlace = FindTileForHeight(h, biome.terrainTypes);
                }

                // 4. Lưu vào mảng batch
                positions[index] = new Vector3Int(chunkWorldX + x, chunkWorldY + y, 0);
                tiles[index] = tileToPlace; // Sẽ là null nếu biome hoặc tile không tìm thấy
                index++;
            }
        }

        // 5. Đặt tất cả tile MỘT LẦN (rất nhanh)
        tilemap.SetTiles(positions, tiles);
    }


    /// <summary>
    /// Tìm ra loại Tile phù hợp với giá trị độ cao (noise) từ một danh sách.
    /// </summary>
    /// <param name="heightValue">Giá trị chiều cao (0-1)</param>
    /// <param name="terrainTypes">Danh sách các loại địa hình để tìm kiếm</param>
    /// <returns>TileBase phù hợp</returns>
    private TileBase FindTileForHeight(float heightValue, WorldGenerator.TerrainType[] terrainTypes)
    {
        if (terrainTypes == null) return null; // An toàn

        for (int i = 0; i < terrainTypes.Length; i++)
        {
            if (heightValue <= terrainTypes[i].height)
            {
                return terrainTypes[i].tile;
            }
        }

        // Nếu cao hơn tất cả, trả về tile cuối cùng (ví dụ: đỉnh núi)
        if (terrainTypes.Length > 0)
        {
            return terrainTypes[terrainTypes.Length - 1].tile;
        }
        return null;
    }

    #endregion

    #region Management

    /// <summary>
    /// Bật GameObject của chunk.
    /// </summary>
    public void Enable()
    {
        if (!isVisible && chunkObject != null)
        {
            isVisible = true;
            chunkObject.SetActive(true);
        }
    }

    /// <summary>
    /// Tắt GameObject của chunk (tiết kiệm hiệu năng).
    /// </summary>
    public void Disable()
    {
        if (isVisible && chunkObject != null)
        {
            isVisible = false;
            chunkObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hủy hoàn toàn GameObject của chunk (giải phóng bộ nhớ).
    /// </summary>
    public void DestroyChunk()
    {
        if (chunkObject != null)
        {
            GameObject.Destroy(chunkObject);
        }
    }

    #endregion
}