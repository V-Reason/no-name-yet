using RPG2D.Character.Player;
using UnityEngine;

namespace RPG2D.Character.Monster.Piranha
{
    public enum PiranhaMoveDirection
    {
        Left = -1,
        Right = 1,
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class PiranhaAI : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField] private PiranhaMoveDirection initialDirection = PiranhaMoveDirection.Right;

        [Header("捕获")]
        [SerializeField] private Transform bitePoint;
        [SerializeField, Min(0f)] private float releaseCooldown = 0.35f;
        [SerializeField] private Vector2 releaseImpulse = new(0f, 1.5f);

        [Header("屏幕边界")]
        [SerializeField, Range(0f, 0.25f)] private float viewportTurnPadding = 0.02f;

        private Rigidbody2D rb;
        private Collider2D bodyCollider;
        private Renderer bodyRenderer;
        private int moveDirection;
        private float nextCaptureTime;

        public StateMachine CapturedPlayer { get; private set; }
        public bool IsHoldingPlayer => CapturedPlayer != null;
        public bool CanCaptureNow => Time.time >= nextCaptureTime;
        public Transform BitePoint => bitePoint;
        public Vector2 ReleaseImpulse => releaseImpulse;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            bodyRenderer = GetComponentInChildren<Renderer>();
            moveDirection = (int)initialDirection;
            ApplyFacing();
        }

        private void FixedUpdate()
        {
            UpdateMovement();
            UpdateTurnAround();
        }

        /// <summary>
        /// 尝试捕获进入头部触发区的玩家，并请求玩家状态机切换到食人鱼捕获状态。
        /// </summary>
        public bool TryCapture(StateMachine player)
        {
            if (player == null || bitePoint == null || IsHoldingPlayer || !CanCaptureNow)
            {
                return false;
            }

            if (!player.CaptureByPiranha(this, bitePoint))
            {
                return false;
            }

            CapturedPlayer = player;
            return true;
        }

        /// <summary>
        /// 释放当前捕获的玩家，并开启短暂冷却防止立刻重复捕获。
        /// </summary>
        public void ReleaseCapturedPlayer()
        {
            StateMachine player = CapturedPlayer;
            CapturedPlayer = null;
            nextCaptureTime = Time.time + releaseCooldown;

            if (player != null && player.CapturingPiranha == this)
            {
                player.ClearPiranhaCapture();
            }
        }

        /// <summary>
        /// 按当前朝向给刚体设置水平巡逻速度。
        /// </summary>
        private void UpdateMovement()
        {
            rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        }

        /// <summary>
        /// 根据屏幕边缘判断是否需要调头。
        /// </summary>
        private void UpdateTurnAround()
        {
            if (ShouldTurnAroundByCamera())
            {
                TurnAround();
            }
        }

        /// <summary>
        /// 使用主相机视口边缘作为调头条件。
        /// </summary>
        private bool ShouldTurnAroundByCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(GetLeadingEdgePosition());
            if (viewportPosition.z < 0f)
            {
                return false;
            }

            return moveDirection < 0 && viewportPosition.x <= viewportTurnPadding
                || moveDirection > 0 && viewportPosition.x >= 1f - viewportTurnPadding;
        }

        /// <summary>
        /// 获取当前移动方向上的鱼身前端位置，用于更贴近视觉边缘地调头。
        /// </summary>
        private Vector3 GetLeadingEdgePosition()
        {
            Bounds bounds = GetMovementBounds();
            float leadingX = moveDirection > 0 ? bounds.max.x : bounds.min.x;
            return new Vector3(leadingX, bounds.center.y, transform.position.z);
        }

        /// <summary>
        /// 优先使用碰撞体边界，其次使用渲染边界，最后退回根物体位置。
        /// </summary>
        private Bounds GetMovementBounds()
        {
            if (bodyCollider != null)
            {
                return bodyCollider.bounds;
            }

            if (bodyRenderer != null)
            {
                return bodyRenderer.bounds;
            }

            return new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        /// 反转巡逻方向并同步贴图朝向。
        /// </summary>
        private void TurnAround()
        {
            moveDirection *= -1;
            ApplyFacing();
        }

        /// <summary>
        /// 根据移动方向翻转本体缩放，保证视觉朝向与巡逻方向一致。
        /// </summary>
        private void ApplyFacing()
        {
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);
            scale.x = (absX <= Mathf.Epsilon ? 1f : absX) * moveDirection;
            transform.localScale = scale;
        }
    }
}
