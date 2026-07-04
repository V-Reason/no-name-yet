using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelView : MonoBehaviour
{
    [Header("UI 引用")]
    public Button menuButton;

    private void Awake()
    {
        menuButton.onClick.AddListener(BackToMenu);
    }

    private void BackToMenu()
    {
        LevelManager.Instance.RestartCurrentLevel();
    }
}
