using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D _rb;
    [SerializeField] protected CharacterData _characterData;
    [SerializeField] private SpriteRenderer spriteRen;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        spriteRen = GetComponent<SpriteRenderer>();
    }


    protected virtual void FixedUpdate() { }

    protected virtual void Move(float _moveSpeed) { }

    // Hàm này được ReferenceManager gọi
    public void SetCharacterData(CharacterData data)
    {
        _characterData = data;

        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        if (_characterData == null) return;

        if (_characterData.RoleName == "Bio-Engineer")
        {
            spriteRen.color = Color.green;
        }
        else if (_characterData.RoleName == "Neuro-Scientist")
        {
            spriteRen.color = Color.blue;
        }

        Debug.Log($"[PlayerController] Setup hoàn tất với Role: {_characterData.RoleName}");
    }
}