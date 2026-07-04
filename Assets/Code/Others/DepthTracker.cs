using UnityEngine;

public class DepthTracker : MonoBehaviour
{
    public Transform playerTransform;
    private float startY;
    private bool isTracking;

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
        if (state == GameState.Playing)
        {
            startY = playerTransform.position.y;
            isTracking = true;
        }
        else
        {
            isTracking = false;
        }
    }

    void Update()
    {
        if (!isTracking || playerTransform == null) return;

        float depth = startY - playerTransform.position.y;
        Blackboard.Instance.currentDepth = Mathf.Max(0, depth);

        if (Blackboard.Instance.currentDepth >= Blackboard.Instance.targetDepth)
        {
            isTracking = false;
            GameManager.Instance.EndGame();
        }
    }
}
