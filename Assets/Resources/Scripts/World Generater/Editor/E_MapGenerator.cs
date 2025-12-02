using UnityEditor;
using UnityEngine;

/// <summary>
/// (Chỉ dùng cho Editor) Tùy chỉnh Inspector cho script WorldGenerator.
/// Thêm nút "Redraw Map" và bật tính năng "autoUpdate".
/// </summary>
[CustomEditor(typeof(WorldGenerator))]
public class E_MapGenerator : Editor
{
    /// <summary>
    /// Vẽ Inspector tùy chỉnh.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Lấy đối tượng WorldGenerator đang được chọn
        WorldGenerator worldGenerator = (WorldGenerator)target;

        // 1. Vẽ tất cả các biến public/serializefield mặc định
        // Nếu DrawDefaultInspector() trả về true, nghĩa là người dùng vừa thay đổi 1 giá trị
        if (DrawDefaultInspector())
        {
            // Nếu bật autoUpdate, tự động gọi GenerateMap()
            if (worldGenerator.autoUpdate)
            {
                worldGenerator.GenerateMap();
            }
        }

        // 2. Vẽ một nút "Redraw Map"
        if (GUILayout.Button("Redraw Map"))
        {
            // Khi nhấn nút, gọi GenerateMap()
            worldGenerator.GenerateMap();
        }
    }
}