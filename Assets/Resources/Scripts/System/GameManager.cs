using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState { MainMenu, Gameplay, Paused, GameOver }

[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{
    //===== Fields =====//
    public GameState CurrentState { get; private set; }

    //===== Override Methods =====//
    protected override void OnAwake() { }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                break;
            case GameState.Gameplay:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
        }

        Debug.Log($"[GameManager] State changed to: {newState}");
    }

    //===== Scene Load Events =====//
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SystemSetup")
        {
            SceneManager.LoadScene("MainMenu");
        }
        else if (scene.name == "MainMenu")
        {
            ChangeState(GameState.MainMenu);
        }
    }

    //===== Functions =====//
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}