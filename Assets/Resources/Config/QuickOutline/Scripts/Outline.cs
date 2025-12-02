using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class Outline : MonoBehaviour
{
    [Header("Shader Graph Settings")]
    // Nếu bạn dùng Resources.Load thì nhập đường dẫn (ví dụ: "Shaders/SpriteOutline")
    // Nếu dùng Shader.Find thì nhập tên trong Graph (ví dụ: "Shader Graphs/SpriteOutline")
    [SerializeField] private string shaderName = "Shader Graphs/SpriteOutline";

    [Header("Outline Settings")]
    public Color OutlineColor = Color.white;
    [Range(0.0001f, 0.1f)] public float OutlineWidth = 0.01f;
    public bool ShowOutline = true;

    private SpriteRenderer _spriteRenderer;
    private Material _outlineMaterial;
    private Material _defaultMaterial;

    // Các ID tham chiếu vào Shader Graph
    private static readonly int _MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int _OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int _OutlineWidthID = Shader.PropertyToID("_OutlineThickness");
    private static readonly int _OutlineAlphaID = Shader.PropertyToID("_OutlineAlpha");

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultMaterial = _spriteRenderer.sharedMaterial;

/*        //Log thông số
        Debug.Log($"_MainTexTD: {_MainTexID}");
        Debug.Log($"_OutlineColorID: {_OutlineColorID}");
        Debug.Log($"_OutlineWidthID: {_OutlineWidthID}");
        Debug.Log($"_OutlineAlphaID: {_OutlineAlphaID}");
*/
        // 1. Tìm Shader (Ưu tiên dùng Resources.Load nếu file nằm trong folder Resources để tránh lỗi mất shader khi build)
        // Nếu bạn dùng Shader.Find, hãy đảm bảo shader đã được đưa vào danh sách "Always Included Shaders" trong Project Settings
        Shader shader = Shader.Find(shaderName);

        // Fallback: Thử tìm bằng Resources nếu Find thất bại
        if (shader == null) shader = Resources.Load<Shader>(shaderName);

        if (shader == null)
        {
            Debug.LogError($"[Outline] Không tìm thấy Shader: '{shaderName}'.");
            return;
        }

        // 2. Tạo Material Instance mới
        _outlineMaterial = new Material(shader);

        // 3. Tự động gán Texture hiện tại làm Default Value ngay lập tức
        if (_spriteRenderer.sprite != null)
        {
            // Lấy texture của sprite hiện tại gán vào MainTex của shader
            _outlineMaterial.SetTexture(_MainTexID, _spriteRenderer.sprite.texture);

            //Debug.Log($"[Outline] Đã gán texture mặc định cho Outline Material từ Sprite hiện tại. {_outlineMaterial.GetTexture(_MainTexID)}");
        }
    }

    void OnEnable()
    {
        if (_outlineMaterial != null)
        {
            _spriteRenderer.material = _outlineMaterial;

            // Gọi update ngay khi bật để đảm bảo thông số đúng
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
        // Vẫn cần Update liên tục để hỗ trợ Animation (Sprite thay đổi hình ảnh)
        UpdateOutline();
    }

    void UpdateOutline()
    {
        if (_outlineMaterial == null || _spriteRenderer.sprite == null) return;

        Texture currentSpriteTexture = _spriteRenderer.sprite.texture;

        // Kiểm tra xem texture có thay đổi không (do Animation) thì mới gán lại cho nhẹ
        if (_outlineMaterial.GetTexture(_MainTexID) != currentSpriteTexture)
        {
            _outlineMaterial.SetTexture(_MainTexID, currentSpriteTexture);
        }

        _outlineMaterial.SetColor(_OutlineColorID, OutlineColor);
        _outlineMaterial.SetFloat(_OutlineWidthID, OutlineWidth);
        _outlineMaterial.SetFloat(_OutlineAlphaID, ShowOutline ? 1f : 0f);
    }

    void OnDestroy()
    {
        if (_outlineMaterial != null) Destroy(_outlineMaterial);
    }
}