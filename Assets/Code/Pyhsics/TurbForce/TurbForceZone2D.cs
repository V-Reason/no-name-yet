using UnityEngine;

namespace RPG2D.Pyhsics.TurbForce
{
    /// <summary>
    /// 固定 2D 湍流力区域, 为进入区域的接收器提供固定方向和强度的力.
    /// </summary>
    public class TurbForceZone2D : MonoBehaviour
    {
        [SerializeField]
        private Vector2 forceDirection = Vector2.up;

        [SerializeField, Min(0f)]
        private float forceMagnitude = 10f;

        /// <summary>
        /// 获取当前区域提供的湍流力, 方向会被归一化后再乘以强度.
        /// </summary>
        public Vector2 Force
        {
            get
            {
                if (forceDirection.sqrMagnitude <= Mathf.Epsilon || forceMagnitude <= 0f)
                {
                    return Vector2.zero;
                }

                return forceDirection.normalized * forceMagnitude;
            }
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void Awake()
        {
            EnsureTriggerCollider();
        }

        private void OnValidate()
        {
            forceMagnitude = Mathf.Max(0f, forceMagnitude);
            EnsureTriggerCollider();
        }

        /// <summary>
        /// 设置湍流方向和强度, 用于运行时配置或测试固定力结果.
        /// </summary>
        public void SetForce(Vector2 direction, float magnitude)
        {
            forceDirection = direction;
            forceMagnitude = Mathf.Max(0f, magnitude);
        }

        /// <summary>
        /// 确保区域碰撞体以 Trigger 形式工作, 避免湍流区域阻挡对象移动.
        /// </summary>
        private void EnsureTriggerCollider()
        {
            Collider2D zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
            {
                return;
            }

            zoneCollider.isTrigger = true;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 force = Force;
            if (force.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)force.normalized;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.08f);
        }
    }
}
