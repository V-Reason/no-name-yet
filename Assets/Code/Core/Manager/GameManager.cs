using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Menu;

    void Start()
    {
        ChangeState(GameState.Menu);
    }

    void Update()
    {
        if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        // 处理状态进入时的逻辑
        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 0;
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                break;
            case GameState.Paused:
                Time.timeScale = 0; // 暂停游戏物理和计时
                break;
            case GameState.GameOver:
                Time.timeScale = 0.5f; // 结算时给一点慢镜头效果
                break;
        }

        if (CurrentState == newState) return;
        CurrentState = newState;

        // 广播状态切换事件
        EventCenter.Broadcast(EventType.GameStateChanged, newState);
    }

    // 快捷方法
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing) ChangeState(GameState.Paused);
        else if (CurrentState == GameState.Paused) ChangeState(GameState.Playing);
    }

    public void EndGame()
    {
        ChangeState(GameState.GameOver);
    }
}
