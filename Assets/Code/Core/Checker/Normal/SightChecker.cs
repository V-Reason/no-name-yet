using UnityEngine;

namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 通用视线检测
    /// 一：圆形范围筛选目标
    /// 二：长线段遮挡检测
    /// </summary>
    public class SightChecker : BaseChecker
    {
        [Header("视线范围设置")]
        [Tooltip("检测半径范围")]
        [SerializeField] private float circleRadius = 5.0f;
        [Tooltip("射线起点位置")]
        [SerializeField] private Vector2 rayOffset = Vector2.zero;
        [Tooltip("是否跟随物体朝向自动翻转偏移量")]
        [SerializeField] private bool autoFlipOffset = true;
        [Space]
        [Header("必须手动设置")]
        [Tooltip("需要检测的目标图层")]
        [SerializeField] private LayerMask targetLayer;
        [Tooltip("会遮挡射线的障碍物图层")]
        [SerializeField] private LayerMask obstacleLayer;

        // 门面模式，对外接口
        // 检测到的目标引用
        [HideInInspector]
        public Collider2D detectedTarget { get; protected set; }
        // 是否能看见目标
        [HideInInspector]
        public bool canSeeTarget
        {
            get => IsConditionMet;
            protected set => IsConditionMet = value;
        }

        public override void Check()
        {
            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1;
            Vector2 localOffset = new Vector2(rayOffset.x * facing, rayOffset.y);
            Vector2 rayPos = (Vector2)transform.position + localOffset;

            // 阶段一：圆形检测，目标是否进入可能的探测范围
            Collider2D target = Physics2D.OverlapCircle(rayPos, circleRadius, targetLayer);
            if (target != null)
            {
                // 阶段二：射线检测，目标是否直线可达
                Vector2 targetPos = target.bounds.center;
                RaycastHit2D hit = Physics2D.Linecast(rayPos, targetPos, obstacleLayer);
                if (hit.collider == null)
                {
                    detectedTarget = target;
                    canSeeTarget = true;
                    return;
                }
            }

            detectedTarget = null;
            canSeeTarget = false;
        }

#if UNITY_EDITOR
        [Header("检测Gizmos")]
        [SerializeField]
        private bool drawGizmos = true;
        [Header("辅助线参数")]
        [SerializeField] private Color untouchedColor = Color.red;
        [SerializeField] private Color touchedColor = Color.green;
        [SerializeField] private float wireSphereRadius = 0.05f;
        [Range(0f, 1f)]
        [SerializeField] private float gizmosAlpha = 1f;

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1f;
            Vector2 localOffset = new Vector2(rayOffset.x * facing, rayOffset.y);
            Vector2 rayPos = (Vector2)transform.position + localOffset;

            Color baseColor = canSeeTarget ? touchedColor : untouchedColor;
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, gizmosAlpha);
            Gizmos.DrawWireSphere(rayPos, wireSphereRadius);
            Gizmos.DrawWireSphere(rayPos, circleRadius);

            Collider2D target = Physics2D.OverlapCircle(rayPos, circleRadius, targetLayer);
            if (target != null)
            {
                Vector2 targetPos = target.bounds.center;
                RaycastHit2D hit = Physics2D.Linecast(rayPos, targetPos, obstacleLayer);

                Gizmos.color = (hit.collider == null) ? Color.green : Color.red;
                Gizmos.DrawLine(rayPos, targetPos);
            }
        }
#endif
    }
}