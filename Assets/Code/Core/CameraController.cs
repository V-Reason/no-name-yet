using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform playerTransform;

    private Transform _menuTarget;

    private void Awake()
    {
        // 在 (0,0) 创建一个不可见的虚拟目标，用于菜单阶段
        var go = new GameObject("__MenuCameraTarget__");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.position = Vector3.zero;
        _menuTarget = go.transform;
    }

    private void Start()
    {
        virtualCamera.Follow = _menuTarget;
    }

    private void OnEnable()
    {
        EventCenter.Subscribe(EventType.GameStateChanged, OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventCenter.Unsubscribe(EventType.GameStateChanged, OnGameStateChanged);
    }

    private void OnGameStateChanged(object state)
    {
        var gameState = (GameState)state;

        if (gameState == GameState.Playing && playerTransform != null)
        {
            virtualCamera.Follow = playerTransform;
        }
        else if (gameState == GameState.Menu)
        {
            virtualCamera.Follow = _menuTarget;
        }
    }
}
