using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private CharacterData _characterData => ReferenceManager.Instance.characterData;
    [SerializeField] private SpriteRenderer spriteRen;

    private void Awake()
    {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializeCharacter();

    }


    /// <summary>
    /// Hàm này được ReferenceManager gọi
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