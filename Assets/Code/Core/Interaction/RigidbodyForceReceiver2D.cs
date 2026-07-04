using UnityEngine;

namespace RPG2D.Core.Interaction
{
    /// <summary>
    /// 将普通 Rigidbody2D 适配为可被外部力场统一施力的对象。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidbodyForceReceiver2D : MonoBehaviour, IForceReceiver, IForceReceiverVelocitySource
    {
        [SerializeField]
        private Rigidbody2D rb;

        public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;

        /// <summary>
        /// 对绑定刚体施加持续力，缺少刚体或静态刚体时安全跳过。
        /// </summary>
        public void ApplyForce(Vector2 force)
        {
            EnsureRigidbody();
            if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            {
                return;
            }

            rb.AddForce(force, ForceMode2D.Force);
        }

        /// <summary>
        /// 初始化时缓存刚体引用，避免每次受力时重复查找组件。
        /// </summary>
        private void Awake()
        {
            EnsureRigidbody();
        }

        /// <summary>
        /// 添加组件时自动补齐刚体引用，方便在 Inspector 中检查。
        /// </summary>
        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// 编辑器改动后同步刚体引用，防止序列化字段为空。
        /// </summary>
        private void OnValidate()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }
        }

        /// <summary>
        /// 确保刚体引用可用，组件异常配置时输出可定位的警告。
        /// </summary>
        private void EnsureRigidbody()
        {
            if (rb != null)
            {
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning($"{nameof(RigidbodyForceReceiver2D)} requires a Rigidbody2D.", this);
            }
        }
    }
}
