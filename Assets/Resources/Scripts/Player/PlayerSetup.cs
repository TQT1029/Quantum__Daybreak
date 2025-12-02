using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] protected CharacterData _characterData => ReferenceManager.Instance.characterData;
    [SerializeField] private SpriteRenderer spriteRen;

    protected void Awake()
    {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Hàm này được ReferenceManager gọi để nạp dữ liệu nhân vật bằng cấu trúc kế thừa
    /// </summary>

    /*
        public void SetCharacterData(CharacterData data)
        {
            _characterData = data;

            InitializeCharacter();
        }

    */
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