using UnityEngine;

/// <summary>
/// Sử dụng ScriptableObject để định nghĩa một Biome.
/// Mỗi file Biome (ví dụ: "DesertBiome.asset") sẽ chứa một danh sách các loại địa hình riêng.
/// </summary>
[CreateAssetMenu(fileName = "New Biome", menuName = "World Generation/Biome")]
public class Biome : ScriptableObject
{
	[Header("Biome Info")]
	[Tooltip("Tên của Biome, ví dụ: Rừng, Sa mạc...")]
	public string biomeName;

	[Header("Terrain Types")]
	[Tooltip("Danh sách các loại địa hình (và tile tương ứng) CỦA RIÊNG BIOME NÀY.")]
	public WorldGenerator.TerrainType[] terrainTypes;

	[Tooltip("Biome đứng trước biome này")] 
	public Biome biomeBefore;

    //Chiều cao cao nhất của biome trước và chiều cao cao nhất của biome này
    public float minHeight => biomeBefore != null ? biomeBefore.terrainTypes[biomeBefore.terrainTypes.Length - 1].height : 0f;
    public float maxHeight => terrainTypes.Length > 0 ? terrainTypes[terrainTypes.Length - 1].height : 1f;
}