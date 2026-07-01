using UnityEngine;

namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 圆形重叠检测器
    /// </summary>
    public class CircleChecker : BaseChecker
    {
        [Header("圆型检测器参数")]
        [Tooltip("检测半径")]
        [SerializeField] protected float radius = 2.0f;
        [Tooltip("原点偏移量")]
        [SerializeField] protected Vector2 originOffset = Vector2.zero;
        [Tooltip("是否跟随物体朝向自动翻转偏移量")]
        [SerializeField] protected bool autoFlipOffset = true;
        [Space]
        [Header("必须设置引用")]
        [Tooltip("检测层")]
        [SerializeField] protected LayerMask targetLayer;

        // 门面模式，对外接口
        // 命中数量
        [HideInInspector]
        public int hitCnt { get; protected set; } = 0;
        // 检测到的第一个碰撞体引用
        [HideInInspector]
        public Collider2D firstDetectedCollider { get; protected set; }
        // 是否重叠
        [HideInInspector]
        public bool overlapped
        {
            get => IsConditionMet;
            protected set => IsConditionMet = value;
        }

        public override void Check()
        {
            // 自动根据本地朝向计算计算偏移量
            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1;
            Vector2 localOffset = new Vector2(originOffset.x * facing, originOffset.y);
            Vector2 origin = (Vector2)transform.position + localOffset;

            // 执行检测
            Collider2D hit = Physics2D.OverlapCircle(origin, radius, targetLayer);
            if (hit != null)
            {
                hitCnt = 1;
                firstDetectedCollider = hit;
                overlapped = true;
            }
            else
            {
                hitCnt = 0;
                firstDetectedCollider = null;
                overlapped = false;
            }
        }

#if UNITY_EDITOR
        [Header("检测Gizmos")]
        [SerializeField] protected bool drawGizmos = true;
        // 辅助线参数
        [Header("辅助线参数")]
        [Tooltip("未接触颜色")]
        [SerializeField] protected Color untouchedColor = Color.red;
        [Tooltip("接触颜色")]
        [SerializeField] protected Color touchedColor = Color.green;
        [Tooltip("中心原点提示圈半径")]
        [SerializeField] protected float wireSphereRadius = 0.05f;
        [Range(0f, 1f)]
        [SerializeField] protected float gizmosAlpha = 1f;

        // 绘制辅助检测圆
        protected virtual void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1f;
            Vector2 localOffset = new Vector2(originOffset.x * facing, originOffset.y);
            Vector2 origin = (Vector2)transform.position + localOffset;

            Color baseColor = overlapped ? touchedColor : untouchedColor;
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, gizmosAlpha);

            Gizmos.DrawWireSphere(origin, wireSphereRadius);
            Gizmos.DrawWireSphere(origin, radius);
        }
#endif
    }
}