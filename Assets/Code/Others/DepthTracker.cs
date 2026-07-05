using UnityEngine;

public class DepthTracker : MonoBehaviour
{
    public Transform playerTransform;
    private float startY;
    private bool isTracking;
    private bool hasStarted;
    private bool midCheckpointTriggered;

    void OnEnable()
    {
        EventCenter.Subscribe(EventType.GameStateChanged, OnStateChanged);
    }

    void OnDisable()
    {
        EventCenter.Unsubscribe(EventType.GameStateChanged, OnStateChanged);
    }

    private void OnStateChanged(object data)
    {
        GameState state = (GameState)data;
        switch (state)
        {
            case GameState.Playing:
                if (playerTransform == null) return;
                if (!hasStarted)
                {
                    startY = playerTransform.position.y;
                    hasStarted = true;
                }
                isTracking = true;
                break;
            case GameState.MidCheckpoint:
                break;
            case GameState.Menu:
                isTracking = false;
                hasStarted = false;
                midCheckpointTriggered = false;
                break;
            default:
                isTracking = false;
                break;
        }
    }

    void Update()
    {
        if (!isTracking || playerTransform == null) return;

        float rawDepth = startY - playerTransform.position.y;
        float displayDepth = rawDepth * Blackboard.Instance.depthMultiplier;
        Blackboard.Instance.currentDepth = Mathf.Max(0, displayDepth);

        if (!midCheckpointTriggered && Blackboard.Instance.currentDepth >= 250f)
        {
            midCheckpointTriggered = true;
            GameManager.Instance.ChangeState(GameState.MidCheckpoint);
            return;
        }

        if (Blackboard.Instance.currentDepth >= Blackboard.Instance.targetDepth)
        {
            isTracking = false;
            GameManager.Instance.EndGame();
        }
    }
}
