using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-9)]
public class ReferenceManager : Singleton<ReferenceManager>
{
    [Header("Global References")]
    public Camera MainCamera;
    public Transform PlayerTransform;
    public CharacterData characterData;

    // Tham chiếu đến WorldGenerator hiện tại để các script khác (AI, ChunkManager) truy cập
    public WorldGenerator ActiveWorld;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MainCamera = Camera.main;

        // Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerTransform = playerObj.transform;
        }
        else
        {
            PlayerTransform = null;
        }

        // Load Data Nhân vật
        LoadCharacterData();

        // Setup Player Controller
        if (PlayerTransform != null && characterData != null)
        {
            PlayerController controller = PlayerTransform.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetCharacterData(characterData);
            }
        }

        ActiveWorld = FindFirstObjectByType<WorldGenerator>();

        Debug.Log($"[ReferenceManager] Scene: {scene.name} | Player Found: {PlayerTransform != null} | Map Found: {ActiveWorld != null}");
    }

    private void LoadCharacterData()
    {
        RoleType currentRole = UIManager.Instance.SelectedRole;

        string path = $"Assets/Data/Characters/{currentRole}";

        characterData = Resources.Load<CharacterData>(path);

        if (characterData == null)
        {
            Debug.LogError($"[ReferenceManager] LỖI: Không tìm thấy file tại Resources/{path}. Hãy kiểm tra lại folder Resources!");
        }
    }
}