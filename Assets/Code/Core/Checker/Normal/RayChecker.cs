using Unity.VisualScripting;
using UnityEngine;

namespace RPG2D.Core.Checker
{
    public class RayChecker : BaseChecker
    {
        // 内部参数
        [Header("射线参数设置")]
        [Tooltip("检测长度")]
        [SerializeField] private float rayDistance = 0.15f;
        [Tooltip("射线起点偏移量")]
        [SerializeField] private Vector2 rayOffset = Vector2.zero;
        [Tooltip("检测方向")]
        [SerializeField] private Vector2 rayDirection = Vector2.down;
        [Tooltip("是否启用双射线检测")]
        [SerializeField] private bool useDoubleRay = false;
        [Tooltip("双射线间距，0则用一条射线，否则用两条")]
        [SerializeField] private float rayGap = 0.0f;
        [Tooltip("双射线之间的夹角")]
        [SerializeField] private float rayAngle = 0.0f;
        [Tooltip("是否跟随物体朝向自动翻转偏移量")]
        [SerializeField] private bool autoFlipOffset = true;
        [Space]
        [Header("必须设置引用")]
        [Tooltip("检测层")]
        [SerializeField] private LayerMask targetLayer;

        // 门面模式，对外接口
        // 命中数
        [HideInInspector]
        public int hitCnt = 0;
        //法线
        [HideInInspector]
        public Vector2 normal { get; protected set; } = Vector2.zero;
        //是否接触
        [HideInInspector]
        public bool isTouched
        {
            get => IsConditionMet;
            protected set => IsConditionMet = value;
        }

        public override void Check()
        {
            // 获取物体朝向
            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1;
            // 自动根据朝向计算
            Vector2 localOffset = new Vector2(rayOffset.x * facing, rayOffset.y);
            Vector2 localDirection = new Vector2(rayDirection.x * facing, rayDirection.y);

            Vector2 origin = (Vector2)transform.position + localOffset;   // 射线起点
            Vector2 normalSum = Vector2.zero;   // 法线缓存值
            bool touched = false;               // 缓存值
            hitCnt = 0;                         // 射线计量

            // 一条射线
            if (!useDoubleRay)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, localDirection, rayDistance, targetLayer);
                if (hit.collider != null)
                {
                    hitCnt = 1;
                    normalSum = hit.normal;
                    touched = true;
                }
            }
            // 两条射线
            else
            {
                Vector2 leftOrigin = origin + Vector2.left * rayGap * 0.5f;
                Vector2 rightOrigin = origin + Vector2.right * rayGap * 0.5f;
                Vector2 leftDirection = Quaternion.Euler(0, 0, rayAngle / 2f) * localDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -rayAngle / 2f) * localDirection;
                RaycastHit2D leftHit
                        = Physics2D.Raycast(leftOrigin, leftDirection, rayDistance, targetLayer);
                RaycastHit2D rightHit
                        = Physics2D.Raycast(rightOrigin, rightDirection, rayDistance, targetLayer);
                if (leftHit.collider != null)
                {
                    normalSum += leftHit.normal;
                    ++hitCnt;
                }
                if (rightHit.collider != null)
                {
                    normalSum += rightHit.normal;
                    ++hitCnt;
                }
            }

            // 计算法线
            if (hitCnt > 0)
            {
                touched = true;
                normal = (normalSum / hitCnt).normalized;
            }

            // 检测结束
            isTouched = touched;
            if (!isTouched) normal = Vector2.zero;
        }

        // // 镜像翻转射线
        // public void FlipRay()
        // {
        //     rayOffset = new Vector2(-rayOffset.x, rayOffset.y);
        //     rayDirection = new Vector2(-rayDirection.x, rayDirection.y);
        //     rayAngle = -rayAngle;
        // }

#if UNITY_EDITOR
        [Header("检测Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        // 辅助线参数
        [Header("辅助线参数")]
        [Tooltip("未接触颜色")]
        [SerializeField] private Color untouchedColor = Color.red;
        [Tooltip("接触颜色")]
        [SerializeField] private Color touchedColor = Color.green;
        [Tooltip("原点提示圈半径")]
        [SerializeField] private float wireSphereRadius = 0.05f;
        [Range(0f, 1f)]
        [SerializeField] private float gizmosAlpha = 1f;

        // 绘制辅助线
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1;
            Vector2 localOffset = new Vector2(rayOffset.x * facing, rayOffset.y);
            Vector2 localDirection = new Vector2(rayDirection.x * facing, rayDirection.y);

            Vector2 origin = (Vector2)transform.position + localOffset;

            Color baseColor = isTouched ? touchedColor : untouchedColor;
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, gizmosAlpha);

            if (!useDoubleRay)
            {
                Gizmos.DrawWireSphere(origin, wireSphereRadius);
                Gizmos.DrawLine(origin, origin + localDirection * rayDistance);
            }
            else
            {
                Vector2 leftOrigin = origin + Vector2.left * rayGap * 0.5f;
                Vector2 rightOrigin = origin + Vector2.right * rayGap * 0.5f;
                Vector2 leftDirection = Quaternion.Euler(0, 0, rayAngle / 2f) * localDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -rayAngle / 2f) * localDirection;
                Gizmos.DrawWireSphere(leftOrigin, wireSphereRadius);
                Gizmos.DrawWireSphere(rightOrigin, wireSphereRadius);
                Gizmos.DrawLine(leftOrigin, leftOrigin + leftDirection * rayDistance);
                Gizmos.DrawLine(rightOrigin, rightOrigin + rightDirection * rayDistance);
            }
        }
#endif
    }
}