using UnityEngine;
using UnityEngine.UI;

public class MenuPanelView : MonoBehaviour
{
    [Header("UI 引用")]
    public Button startButton;
    public Button quitButton;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
        startButton.interactable = false;
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
