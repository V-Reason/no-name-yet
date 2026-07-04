using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Core.Checker
{
    public class InteractableChecker : CircleChecker
    {
        [SerializeField, Min(1)] private int maxBufferCount = 12;

        private Collider2D[] resultsBuffer;

        public IGrabbable detectedGrabbable { get; private set; }

        private void Awake()
        {
            EnsureBuffer();
        }

        public override void Check()
        {
            EnsureBuffer();

            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1f;
            Vector2 localOffset = new Vector2(originOffset.x * facing, originOffset.y);
            Vector2 origin = (Vector2)transform.position + localOffset;

            hitCnt = Physics2D.OverlapCircleNonAlloc(origin, radius, resultsBuffer, targetLayer);
            detectedGrabbable = null;
            firstDetectedCollider = null;
            overlapped = false;

            float closestSqrDistance = float.MaxValue;
            for (int i = 0; i < hitCnt; i++)
            {
                Collider2D hit = resultsBuffer[i];
                IGrabbable grabbable = ResolveGrabbable(hit);
                if (grabbable == null || !grabbable.CanGrab())
                {
                    continue;
                }

                float sqrDistance = GetSqrDistanceToCollider(origin, hit);
                if (sqrDistance >= closestSqrDistance)
                {
                    continue;
                }

                closestSqrDistance = sqrDistance;
                detectedGrabbable = grabbable;
                firstDetectedCollider = hit;
                overlapped = true;
            }
        }

        /// <summary>
        /// 确保检测缓存已按当前配置创建, 避免每帧交互检测产生临时数组.
        /// </summary>
        private void EnsureBuffer()
        {
            maxBufferCount = Mathf.Max(1, maxBufferCount);
            if (resultsBuffer == null || resultsBuffer.Length != maxBufferCount)
            {
                resultsBuffer = new Collider2D[maxBufferCount];
            }
        }

        /// <summary>
        /// 从命中的碰撞体解析可抓取对象, 兼容碰撞体挂在子物体而接口挂在父物体的结构.
        /// </summary>
        private static IGrabbable ResolveGrabbable(Collider2D hit)
        {
            if (hit == null)
            {
                return null;
            }

            IGrabbable grabbable = hit.GetComponent<IGrabbable>();
            return grabbable ?? hit.GetComponentInParent<IGrabbable>();
        }

        /// <summary>
        /// 计算检测圆心到碰撞体的近似距离平方, 用于在多个可抓目标中选择最近者.
        /// </summary>
        private static float GetSqrDistanceToCollider(Vector2 origin, Collider2D hit)
        {
            if (hit == null)
            {
                return float.MaxValue;
            }

            Vector2 closestPoint = hit.ClosestPoint(origin);
            return (closestPoint - origin).sqrMagnitude;
        }
    }
}
