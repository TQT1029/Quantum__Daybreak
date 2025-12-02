using UnityEngine;

/// <summary>
/// Script này CHỈ DÙNG CHO EDITOR PREVIEW.
/// Nó lấy dữ liệu map (noise hoặc màu) và hiển thị lên một SpriteRenderer
/// trong Scene View để bạn dễ dàng điều chỉnh thông số.
/// Nó không chạy lúc game build.
/// </summary>
public class MapDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRender;

    // Lưu lại sprite cũ để hủy, tránh rò rỉ bộ nhớ (memory leak) trong Editor
    private Sprite oldSprite;

    #region Draw Functions

    /// <summary>
    /// Vẽ một map XÁM dựa trên giá trị noise (0-1).
    /// </summary>
    public void HeightMap(float[,] noiseMap)
    {
        int chunkSize = noiseMap.GetLength(0);
        Texture2D texture = CreateTexture(chunkSize);
        Color[] pixels = new Color[chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float value = noiseMap[x, y];
                pixels[y * chunkSize + x] = Color.Lerp(Color.white, Color.black, value);
            }
        }

        ApplyPixels(texture, pixels, chunkSize);
    }

    /// <summary>
    /// Vẽ một map MÀU dựa trên giá trị nhiệt độ (0-1).
    /// </summary>
    public void TempMap(float[,] noiseMap)
    {
        int chunkSize = noiseMap.GetLength(0);
        Texture2D texture = CreateTexture(chunkSize);
        Color[] pixels = new Color[chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                pixels[y * chunkSize + x] = TemperatureToColor(noiseMap[x, y]);
            }
        }

        ApplyPixels(texture, pixels, chunkSize);
    }

    /// <summary>
    /// Vẽ một map MÀU dựa trên giá trị độ ẩm (0-1).
    /// </summary>
    public void HumidityMap(float[,] noiseMap)
    {
        int chunkSize = noiseMap.GetLength(0);
        Texture2D texture = CreateTexture(chunkSize);
        Color[] pixels = new Color[chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                pixels[y * chunkSize + x] = HumidityToColor(noiseMap[x, y]);
            }
        }

        ApplyPixels(texture, pixels, chunkSize);
    }

    /// <summary>
    /// Vẽ một map MÀU (thường là Biome) dựa trên mảng colorMap đã tính.
    /// </summary>
    public void DrawColorMap(Color[] colorMap, int chunkSize)
    {
        Texture2D texture = CreateTexture(chunkSize);
        ApplyPixels(texture, colorMap, chunkSize);
    }

    #endregion

    #region Helper Functions

    /// <summary>
    /// Tạo một Texture2D rỗng với cài đặt mặc định.
    /// </summary>
    private Texture2D CreateTexture(int chunkSize)
    {
        Texture2D texture = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    /// <summary>
    /// Áp dụng pixels vào texture và gán nó vào SpriteRenderer.
    /// </summary>
    private void ApplyPixels(Texture2D texture, Color[] pixels, int chunkSize)
    {
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        ApplyTextureToSprite(texture, chunkSize);
    }

    /// <summary>
    /// Hàm lõi: Chuyển Texture2D thành Sprite và gán vào SpriteRenderer.
    /// Xử lý việc dọn dẹp sprite cũ để tránh memory leak trong Editor.
    /// </summary>
    private void ApplyTextureToSprite(Texture2D texture, int chunkSize)
    {
        // RẤT QUAN TRỌNG: Phải hủy Sprite cũ VÀ Texture bên trong nó
        if (oldSprite != null)
        {
            if (Application.isPlaying) // Chế độ Play
            {
                Destroy(oldSprite.texture);
                Destroy(oldSprite);
            }
            else // Chế độ Editor
            {
                DestroyImmediate(oldSprite.texture);
                DestroyImmediate(oldSprite);
            }
        }

        Rect rect = new Rect(0, 0, chunkSize, chunkSize);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelsPerUnit = 1f; // 1 pixel = 1 unit

        Sprite newSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);

        spriteRender.sprite = newSprite;
        oldSprite = newSprite; // Lưu lại để hủy lần sau
    }

    // Các hàm chuyển đổi màu cho preview
    private Color TemperatureToColor(float value)
    {
        if (value < 0.25f) return Color.Lerp(Color.blue, Color.cyan, value / 0.25f);
        if (value < 0.5f) return Color.Lerp(Color.cyan, Color.green, (value - 0.25f) / 0.25f);
        if (value < 0.75f) return Color.Lerp(Color.green, Color.yellow, (value - 0.5f) / 0.25f);
        return Color.Lerp(Color.yellow, Color.red, (value - 0.75f) / 0.25f);
    }

    private Color HumidityToColor(float value)
    {
        if (value < 0.25f) return Color.Lerp(new Color(0.6f, 0.4f, 0.2f), Color.yellow, value / 0.25f);
        if (value < 0.5f) return Color.Lerp(Color.yellow, Color.green, (value - 0.25f) / 0.25f);
        if (value < 0.75f) return Color.Lerp(Color.green, new Color(0.2f, 0.6f, 1f), (value - 0.5f) / 0.25f);
        return Color.Lerp(new Color(0.2f, 0.6f, 1f), Color.blue, (value - 0.75f) / 0.25f);
    }

    #endregion
}