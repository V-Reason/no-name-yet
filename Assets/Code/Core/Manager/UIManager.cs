using UnityEngine;
using System.Collections.Generic;

public class UIManager : Singleton<UIManager>
{
    [Header("Panel Prefabs")]
    public GameObject menuPanel;
    public GameObject playPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    void OnEnable()
    {
        EventCenter.Subscribe(EventType.GameStateChanged, HandleStateUI);
    }

    void OnDisable()
    {
        EventCenter.Unsubscribe(EventType.GameStateChanged, HandleStateUI);
    }

    private void HandleStateUI(object data)
    {
        GameState state = (GameState)data;

        // 全部关掉
        menuPanel?.SetActive(false);
        playPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        // 根据状态开启对应的面板
        switch (state)
        {
            case GameState.Menu: menuPanel?.SetActive(true); break;
            case GameState.Playing: playPanel?.SetActive(true); break;
            case GameState.Paused: pausePanel?.SetActive(true); break;
            case GameState.GameOver: gameOverPanel?.SetActive(true); break;
        }
    }
}