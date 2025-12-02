using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Button_UI : MonoBehaviour
{
    private Button startButton => UIManager.Instance.startButton;
    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainGame");
        Debug.Log("[UIButton] Start Button Clicked. Loading MainGame scene.");
    }

}
