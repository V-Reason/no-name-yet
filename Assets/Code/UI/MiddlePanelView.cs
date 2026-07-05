using UnityEngine;
using UnityEngine.UI;

public class MiddlePanelView : MonoBehaviour
{
    [Header("UI 引用")]
    public Button continueButton;
    public Button menuButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void OnContinueClicked()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    private void OnMenuClicked()
    {
        Time.timeScale = 1.0f;
        LevelManager.Instance.RestartCurrentLevel();
    }
}
