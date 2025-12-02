using UnityEngine;

/// <summary>
/// Lớp tiện ích tĩnh để tạo các bản đồ noise Perlin.
/// Code này được tối ưu để chạy trên luồng nền.
/// </summary>
public static class NoiseGeneration
{
    /// <summary>
    /// Tạo một bản đồ noise 2D.
    /// </summary>
    /// <param name="chunkSize">Kích thước (width/height) của map</param>
    /// <param name="seed">Hạt giống của thế giới</param>
    /// <param name="noiseParams">Struct chứa các cài đặt (scale, octaves...)</param>
    /// <param name="offset">Vị trí offset của chunk trong thế giới</param>
    /// <returns>Một mảng 2D (float[,]) các giá trị noise đã chuẩn hóa 0-1</returns>
    public static float[,] GenerateNoiseMap(int chunkSize, int seed, WorldGenerator.noiseParams noiseParams, Vector2 offset)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[noiseParams.octaves];

        float maxPossibleNoiseHeight = 0f;
        float amplitude = 1f;

        // Tính toán offset cho từng lớp (octave) và biên độ tối đa
        for (int i = 0; i < noiseParams.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleNoiseHeight += amplitude;
            amplitude *= noiseParams.persistence;
        }

        float scale = noiseParams.scale;
        if (scale <= 0) scale = 0.0001f; // Tránh chia cho 0

        float[,] noiseMap = new float[chunkSize, chunkSize];

        // << SỬA LỖI LOGIC CHUẨN HÓA >>
        // Áp dụng amplitudeFactor cho thang đo (max height)
        float normMaxHeight = maxPossibleNoiseHeight * noiseParams.amplitudeFactor;
        float normalizationDivisor = normMaxHeight * 2f;
        if (normalizationDivisor == 0) normalizationDivisor = 0.00001f;

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f; // Giá trị thô

                // Vòng lặp Octave: Cộng dồn các lớp noise
                for (int octave = 0; octave < noiseParams.octaves; octave++)
                {
                    float sampleX = (x + offset.x + octaveOffsets[octave].x) / scale * frequency;
                    float sampleY = (y + offset.y + octaveOffsets[octave].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f; // [-1, 1]
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= noiseParams.persistence;
                    frequency *= noiseParams.lacunarity;
                }

                // 1. Áp dụng amplitudeFactor cho kết quả thô
                noiseHeight *= noiseParams.amplitudeFactor;

                // 2. Chuẩn hóa giá trị noise về [0,1]
                // Công thức: (giá trị + max) / (max * 2)
                noiseMap[x, y] = (noiseHeight + normMaxHeight) / normalizationDivisor;
            }
        }

        return noiseMap;
    }
}