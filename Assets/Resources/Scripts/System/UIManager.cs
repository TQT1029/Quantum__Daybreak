using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RoleType
{
    BioEngineer,
    NeuroScientist
}

[DefaultExecutionOrder(-10)]
public class UIManager : Singleton<UIManager>
{
    //===== Fields =====//
    public RoleType SelectedRole = RoleType.BioEngineer;

    [Header("UI References")]
    public TMP_Dropdown roleDropdown { get; private set; }
    public Button startButton { get; private set; }


    //===== Scene Load Events =====//

    //Đăng ký sự kien load scene
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    //Hủy đăng ký sự kiện load scene
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[UIManager] Scene Loaded: {scene.name}");
        if (scene.name == "MainMenu")
        {
            roleDropdown = GameObject.Find("RoleDropdown").GetComponent<TMP_Dropdown>();
            startButton = GameObject.Find("StartButton").GetComponent<Button>();


            Debug.Log($"[UIManager] {roleDropdown}, {startButton}");
            Debug.Log("[UIManager] MainMenu UI References Assigned.");
        }
    }

    //=====Functions=====//

    /// <summary>
    /// Hàm này dùng cho UI Button ở Main Menu
    /// </summary>
    public void SetRole(int roleIndex)
    {
        SelectedRole = (RoleType)roleIndex;
        Debug.Log($"[UIManager] Selected Role: {SelectedRole}");
    }

}
