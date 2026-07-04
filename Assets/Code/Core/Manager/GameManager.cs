using System.Collections;
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
    public GameState CurrentState { get; private set; }

    [Header("转场设置")]
    public Transform diveCameraTarget;
    public float transitionDuration = 2.0f;
    public Vector3 diveOffset = new Vector3(0, -20, 0);

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
        if (CurrentState == newState) return;
        CurrentState = newState;

        // 处理状态进入时的逻辑
        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1;
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                break;
            case GameState.Paused:
                Time.timeScale = 0; // 暂停游戏物理和计时
                break;
            case GameState.GameOver:
                Time.timeScale = 0.2f; // 结算时给一点慢镜头效果
                break;
        }

        // 广播状态切换事件
        EventCenter.Broadcast(EventType.GameStateChanged, newState);
    }

    public void StartGameWithTransition()
    {
        StartCoroutine(StartDiveRoutine());
    }

    private IEnumerator StartDiveRoutine()
    {
        Transform cam = diveCameraTarget;
        if (cam == null) cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("GameManager: 没有找到摄像机，请设置 diveCameraTarget 或给摄像机加 MainCamera 标签");
            ChangeState(GameState.Playing);
            yield break;
        }

        Vector3 startPos = cam.position;
        Vector3 endPos = startPos + diveOffset;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);
            cam.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        cam.position = endPos;
        ChangeState(GameState.Playing);
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