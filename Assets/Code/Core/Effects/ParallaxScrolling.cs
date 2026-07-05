using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScrolling : MonoBehaviour
{
    [Header("视差系数")]
    [Tooltip("X/Y 独立视差乘数。0 = 固定不动, 1 = 跟随相机, >1 = 比相机快")]
    [SerializeField] private Vector2 parallaxMultiplier = new Vector2(0.5f, 0.5f);

    [Header("平铺设置")]
    [Tooltip("每个方向额外生成的图块数。2 表示 5x5 网格")]
    [SerializeField] private int tileRadius = 2;

    [Header("相机")]
    [SerializeField] private bool autoFindCamera = true;
    [SerializeField] private Camera targetCamera;

    [Header("玩家对齐")]
    [Tooltip("开启后 Start 时自动将背景瞬移至玩家位置，无需手动对齐")]
    [SerializeField] private bool snapToPlayerOnStart = true;
    [Tooltip("用于查找玩家的 Tag")]
    [SerializeField] private string playerTag = "Player";

    private Vector2 tileSize;
    private Vector3 startPosition;
    private Vector3 startCameraPos;
    private int gridSize;
    private bool _pendingSnapToPlayer;
    private bool _initialized;

    private Transform[] tileTransforms;

    private void Start()
    {
        if (autoFindCamera)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError($"[{name}] ParallaxScrolling: 未找到主相机，请在 Inspector 中手动指定。");
                return;
            }
        }

        if (targetCamera == null) return;

        if (snapToPlayerOnStart)
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentState == GameState.Playing)
            {
                SnapToPlayerPosition();
            }
            else
            {
                transform.position = new Vector3(0, 0, transform.position.z);
            }
        }

        var sourceRenderer = GetComponent<SpriteRenderer>();
        if (sourceRenderer.sprite == null)
        {
            Debug.LogError($"[{name}] ParallaxScrolling: SpriteRenderer 未设置 Sprite。");
            return;
        }

        tileSize = sourceRenderer.bounds.size;
        startPosition = transform.position;
        startCameraPos = targetCamera.transform.position;
        gridSize = tileRadius * 2 + 1;

        BuildTileGrid(sourceRenderer);
        tileTransforms = new Transform[transform.childCount];
        for (int i = 0; i < tileTransforms.Length; i++)
            tileTransforms[i] = transform.GetChild(i);

        sourceRenderer.enabled = false;
        _initialized = true;
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
        if (!_initialized) return;

        var gameState = (GameState)state;
        if (gameState == GameState.Playing)
        {
            _pendingSnapToPlayer = true;
        }
        else if (gameState == GameState.Menu)
        {
            transform.position = new Vector3(0, 0, transform.position.z);
            startPosition = transform.position;
            if (targetCamera != null)
                startCameraPos = targetCamera.transform.position;
        }
    }

    private void SnapToPlayerPosition()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            Vector3 playerPos = player.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
        }
    }

    private void BuildTileGrid(SpriteRenderer source)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(transform);
                tile.transform.localPosition = new Vector3(
                    (x - tileRadius) * tileSize.x,
                    (y - tileRadius) * tileSize.y,
                    0f);

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = source.sprite;
                sr.sortingLayerID = source.sortingLayerID;
                sr.sortingOrder = source.sortingOrder;
                sr.color = source.color;
            }
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        if (_pendingSnapToPlayer)
        {
            SnapToPlayerPosition();
            startPosition = transform.position;
            if (targetCamera != null)
                startCameraPos = targetCamera.transform.position;
            _pendingSnapToPlayer = false;
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        Vector3 camDelta = camPos - startCameraPos;

        // 父节点连续视差运动（不做吸附，保留小数偏移）
        transform.position = new Vector3(
            startPosition.x + camDelta.x * parallaxMultiplier.x,
            startPosition.y + camDelta.y * parallaxMultiplier.y,
            startPosition.z);

        // 逐个 Tile 越界回绕：超出范围则平移整数个 gridSize * tileSize
        float wrapX = gridSize * tileSize.x;
        float wrapY = gridSize * tileSize.y;
        float maxDX = tileRadius * tileSize.x + tileSize.x * 0.5f;
        float maxDY = tileRadius * tileSize.y + tileSize.y * 0.5f;

        for (int i = 0; i < tileTransforms.Length; i++)
        {
            Vector3 wPos = tileTransforms[i].position;
            float dx = wPos.x - camPos.x;
            float dy = wPos.y - camPos.y;

            while (dx > maxDX)  { wPos.x -= wrapX; dx -= wrapX; }
            while (dx < -maxDX) { wPos.x += wrapX; dx += wrapX; }
            while (dy > maxDY)  { wPos.y -= wrapY; dy -= wrapY; }
            while (dy < -maxDY) { wPos.y += wrapY; dy += wrapY; }

            tileTransforms[i].position = wPos;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        tileRadius = Mathf.Max(1, tileRadius);
    }
#endif
}
