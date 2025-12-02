using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-9)]
public class ReferenceManager : Singleton<ReferenceManager>
{
    [Header("Tham chiếu toàn cục (Global References)")]
    public Camera MainCamera;
    public Transform PlayerTransform;
    public CharacterData characterData; // Dữ liệu sẽ được nạp tự động

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tự động tìm Camera chính
        MainCamera = Camera.main;

        // Tự động tìm Player dựa trên Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            PlayerTransform = playerObj.transform;
        }
        else
        {
            PlayerTransform = null;
        }

        // Tự động nạp dữ liệu nhân vật dựa trên Role đã chọn
        LoadCharacterData();



        ///<summary>
        /// Không sử dụng Kế thừa trong trường hợp này để tránh rối code.
        /// </summary>
        /* 
                // Cập nhật lại thông số cho Player (nếu đã tìm thấy Player)
                if (PlayerTransform != null && characterData != null)
                {
                    // Tìm script di chuyển và nạp data vào
                    PlayerController controller = PlayerTransform.GetComponent<PlayerController>();
                    if (controller != null)
                    {
                        controller.SetCharacterData(characterData);
                    }
                }
        */

        Debug.LogWarning($"[ReferenceManager] Scene: {scene.name} | Role: {UIManager.Instance.SelectedRole} | Data Loaded: {(characterData != null ? "Success" : "Fail")}");
    }

    private void LoadCharacterData()
    {
        // Lấy Role từ GameManager
        RoleType currentRole = UIManager.Instance.SelectedRole;

        // LƯU Ý: Tên file trong Resources phải trùng KHÍT với tên trong Enum (bao gồm viết hoa/thường)
        string path = $"ScriptableObjects/Data/Characters/{currentRole.ToString()}";

        characterData = Resources.Load<CharacterData>(path);

        if (characterData == null)
        {
            Debug.LogError($"[ReferenceManager] LỖI: Không tìm thấy file tại 'ScriptableObjects/Data/Characters/{path}'. Kiểm tra lại tên file và folder!");
        }
        else
        {
            Debug.Log($"[ReferenceManager] Đã load data: {characterData.name}");
        }
    }
}