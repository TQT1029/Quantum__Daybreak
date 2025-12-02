using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class Outline : MonoBehaviour
{
    [Header("Shader Settings")]
    // Tên chính xác được định nghĩa trong dòng đầu tiên của file Shader
    [SerializeField] private string shaderName = "Shader Graphs/SpriteOutline";

    [Header("Outline Settings")]
    [ColorUsage(true, true)] // Cho phép chọn màu HDR vì Shader dùng [HDR]
    public Color OutlineColor = new Color(0, 3.5f, 16, 1);

    [Range(0.001f, 0.1f)] // Giới hạn theo Range trong Shader
    public float OutlineWidth = 0.005f;

    public bool ShowOutline = true;

    private SpriteRenderer _spriteRenderer;
    private Material _outlineMaterial;
    private Material _defaultMaterial;

    // --- CÁC ID THAM CHIẾU (Đã khớp với Shader của bạn) ---
    private static readonly int _MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int _OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    // Đã xóa _OutlineAlphaID vì shader không có biến này

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultMaterial = _spriteRenderer.sharedMaterial;

        // 1. Tìm Shader
        Shader shader = Shader.Find(shaderName);
        if (shader == null) shader = Resources.Load<Shader>(shaderName);

        if (shader == null)
        {
            Debug.LogError($"[Outline] Không tìm thấy Shader tên: '{shaderName}'");
            return;
        }

        // 2. Tạo Material mới
        _outlineMaterial = new Material(shader);

        // 3. Gán Texture mặc định ngay lập tức
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
            UpdateOutline();
        }
    }

    void OnDisable()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.material = _defaultMaterial;
        }
    }

    void Update()
    {
        UpdateOutline();
    }

    void UpdateOutline()
    {
        if (_outlineMaterial == null || _spriteRenderer.sprite == null) return;

        // 1. Cập nhật Texture (Hỗ trợ Animation)
        Texture currentSpriteTexture = _spriteRenderer.sprite.texture;
        if (_outlineMaterial.GetTexture(_MainTexID) != currentSpriteTexture)
        {
            _outlineMaterial.SetTexture(_MainTexID, currentSpriteTexture);
        }

        // 2. Cập nhật Màu
        _outlineMaterial.SetColor(_OutlineColorID, OutlineColor);

        // 3. Cập nhật Độ dày (Kiêm chức năng Tắt/Bật)
        if (ShowOutline)
        {
            _outlineMaterial.SetFloat(_OutlineThicknessID, OutlineWidth);
        }
        else
        {
            // Shader không có biến Alpha, nên ta tắt viền bằng cách set độ dày về 0
            _outlineMaterial.SetFloat(_OutlineThicknessID, 0f);
        }
    }

    void OnDestroy()
    {
        if (_outlineMaterial != null) Destroy(_outlineMaterial);
    }
}