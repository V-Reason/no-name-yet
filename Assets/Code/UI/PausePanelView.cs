using UnityEngine;
using UnityEngine.UI;

public class PausePanelView : MonoBehaviour
{
    [Header("UI 引用")]
    public Button resumeButton;
    public Button menuButton;
    public Button quitButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        menuButton.onClick.AddListener(OnMenuClicked);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void OnResumeClicked()
    {
        GameManager.Instance.TogglePause();
    }

    private void OnMenuClicked()
    {
        Time.timeScale = 1.0f;
        LevelManager.Instance.RestartCurrentLevel();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
