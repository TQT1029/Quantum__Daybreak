using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using static Unity.Burst.Intrinsics.X86.Avx;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class Outline : MonoBehaviour
{
    [Header("Shader Settings")]
    // Tên shader chính xác dựa trên file .shader bạn gửi
    [SerializeField] private string shaderName = "Shader Graphs/PixelOutline";

    [Header("Outline Settings")]
    [ColorUsage(true, true)] // Cho phép chỉnh màu HDR (phát sáng)
    public Color OutlineColor = new Color(3.7f, 0f, 0f, 1f);

    [Tooltip("Độ dày của viền (Pixel)")]
    [Min(0f)]
    public float OutlineThickness = 1f;

    [Tooltip("Bật để vẽ thêm pixel ở 4 góc chéo (làm viền mượt hơn/đậm hơn)")]
    public bool SampleCorners = false;

    [Tooltip("Tắt/Bật viền")]
    public bool ShowOutline = true;

    // Các biến nội bộ
    private SpriteRenderer _spriteRenderer;
    private Material _outlineMaterial;
    private Material _defaultMaterial;

    // --- CÁC ID PROPERTY (Phải khớp chính xác với Shader PixelOutline) ---
    private static readonly int _MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int _ColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _ThicknessID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int _CornersID = Shader.PropertyToID("CORNERS"); // Tên biến Toggle trong Properties

    // Từ khóa để bật/tắt đoạn code xử lý góc trong HLSL
    private const string KEYWORD_CORNERS_ON = "CORNERS_ON";

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultMaterial = _spriteRenderer.sharedMaterial;

        // 1. Tìm và Load Shader
        Shader shader = Shader.Find(shaderName);
        if (shader == null) shader = Resources.Load<Shader>(shaderName);

        if (shader == null)
        {
            Debug.LogError($"[Outline] Không tìm thấy Shader: '{shaderName}'. Hãy kiểm tra lại tên file!");
            return;
        }

        // 2. Tạo Material Instance mới từ Shader
        _outlineMaterial = new Material(shader);

        // 3. Tự động gán Texture hiện tại của Sprite vào Material ngay lập tức
        // Điều này giúp tránh hiện tượng "hình vuông trắng" ở frame đầu tiên
        if (_spriteRenderer.sprite != null)
        {
            _outlineMaterial.SetTexture(_MainTexID, _spriteRenderer.sprite.texture);
        }
    }

    void OnEnable()
    {
        if (_outlineMaterial != null)
        {
            _spriteRenderer.material = _outlineMaterial;
            UpdateOutline(); // Cập nhật thông số ngay khi bật
        }
    }

    void OnDisable()
    {
        if (_spriteRenderer != null)
        {
            // Trả lại material gốc (mặc định) khi script bị tắt
            _spriteRenderer.material = _defaultMaterial;
        }
    }

    void Update()
    {
        // Gọi liên tục để hỗ trợ Animation (khi sprite thay đổi) và chỉnh sửa realtime
        UpdateOutline();
    }

    void UpdateOutline()
    {
        if (_outlineMaterial == null || _spriteRenderer.sprite == null) return;

        // --- 1. TỰ ĐỘNG CẬP NHẬT TEXTURE ---
        // Nếu Animation thay đổi sprite, ta phải cập nhật texture mới vào Shader
        Texture currentSpriteTexture = _spriteRenderer.sprite.texture;

        if (_outlineMaterial.GetTexture(_MainTexID) != currentSpriteTexture)
        {
            _outlineMaterial.SetTexture(_MainTexID, currentSpriteTexture);
        }

        // --- 2. CẬP NHẬT MÀU SẮC ---
        _outlineMaterial.SetColor(_ColorID, OutlineColor);

        // --- 3. CẬP NHẬT ĐỘ DÀY (Logic Bật/Tắt) ---
        if (ShowOutline)
        {
            _outlineMaterial.SetFloat(_ThicknessID, OutlineThickness);
        }
        else
        {
            // Shader này không có biến Alpha cho viền, nên ta tắt bằng cách set độ dày về 0
            _outlineMaterial.SetFloat(_ThicknessID, 0f);
        }

        // --- 4. CẬP NHẬT SAMPLE CORNERS (Keyword) ---
        // Shader sử dụng [Toggle] và #pragma shader_feature_local _ CORNERS_ON
        // Ta cần set cả giá trị Float và bật/tắt Keyword để Shader biên dịch đúng biến thể
        if (SampleCorners)
        {
            _outlineMaterial.SetFloat(_CornersID, 1f);
            _outlineMaterial.EnableKeyword(KEYWORD_CORNERS_ON);
        }
        else
        {
            _outlineMaterial.SetFloat(_CornersID, 0f);
            _outlineMaterial.DisableKeyword(KEYWORD_CORNERS_ON);
        }
    }

    void OnDestroy()
    {
        // Dọn dẹp memory khi object bị hủy
        if (_outlineMaterial != null) Destroy(_outlineMaterial);
    }
}

/*
### Các tính năng chính của Script mới:

1.  * *Tương thích Shader Pixel:**Đã cập nhật đúng tên biến `_OutlineThickness` (thay vì `_OutlineWidth`) và thêm xử lý biến `CORNERS`.
2.  **Auto Texture Injection:**Ngay khi `Awake`, script lấy texture từ Sprite hiện tại nhét vào Shader. Bạn không cần phải gán tay texture mặc định trong Shader Graph nữa.
3.  **Xử lý Keyword:**Shader của bạn dùng `shader_feature` cho phần `CORNERS`. Script này tự động gọi `EnableKeyword`/`DisableKeyword` để đảm bảo logic vẽ góc hoạt động đúng.
4.  **Hỗ trợ Animation:**Trong hàm `Update`, script liên tục kiểm tra xem Sprite có bị thay đổi (do Animator) không. Nếu có, nó tự cập nhật texture mới vào Shader để viền luôn khớp với hình ảnh.

### Hướng dẫn sử dụng:
1.  Đảm bảo file `.shader` của bạn tên đúng là `GeneratedFromGraph-PixelOutline.shader` và nằm trong Project.
2.  Gắn script `Outline.cs` này vào nhân vật.
3.  Chỉnh `Outline Thickness` (ví dụ: `1` cho viền 1 pixel, `2` cho viền 2 pixel).
4.  Tích vào `Sample Corners` nếu muốn viền dày dặn hơn ở các góc chéo.
5.  Script sẽ tự động lo phần còn lại.
   */