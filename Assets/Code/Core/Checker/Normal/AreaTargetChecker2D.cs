using UnityEngine;

namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 区域目标检测器, 使用当前物体上的 Collider2D 检测指定层级内的目标.
    /// </summary>
    public class AreaTargetChecker2D : BaseChecker
    {
        [Header("区域目标检测器")]
        [Tooltip("最大允许检测到的目标数量, Awake 时会按该值分配缓存.")]
        [SerializeField, Min(1)]
        private int maxBufferCount = 12;

        [Tooltip("可被检测到的目标层级.")]
        [SerializeField]
        private LayerMask targetLayer;

        private Collider2D areaCollider;
        private Collider2D[] resultsBuffer;
        private ContactFilter2D contactFilter;

        public int HitCount { get; private set; }

        public LayerMask TargetLayer => targetLayer;

        /// <summary>
        /// 按检测结果下标读取目标碰撞体, 下标无效时返回 null.
        /// </summary>
        public Collider2D this[int index]
        {
            get
            {
                if (index < 0 || HitCount <= index)
                {
                    return null;
                }

                return resultsBuffer[index];
            }
        }

        protected virtual void Awake()
        {
            areaCollider = GetComponent<Collider2D>();
            resultsBuffer = new Collider2D[maxBufferCount];
            RefreshContactFilter();
            EnsureTriggerCollider();
        }

        private void OnValidate()
        {
            maxBufferCount = Mathf.Max(1, maxBufferCount);
            RefreshContactFilter();
            EnsureTriggerCollider();
        }

        /// <summary>
        /// 使用区域碰撞体检测目标并缓存结果, 不在检测阶段分配新数组.
        /// </summary>
        public override void Check()
        {
            if (areaCollider == null)
            {
                HitCount = 0;
                IsConditionMet = false;
                return;
            }

            HitCount = areaCollider.OverlapCollider(contactFilter, resultsBuffer);
            IsConditionMet = HitCount > 0;
        }

        /// <summary>
        /// 刷新 LayerMask 对应的 ContactFilter2D, 用于限制可检测目标层级.
        /// </summary>
        private void RefreshContactFilter()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(targetLayer);
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = true;
        }

        /// <summary>
        /// 确保区域碰撞体作为触发范围使用, 避免检测区域阻挡目标移动.
        /// </summary>
        private void EnsureTriggerCollider()
        {
            Collider2D collider = areaCollider != null ? areaCollider : GetComponent<Collider2D>();
            if (collider == null)
            {
                return;
            }

            collider.isTrigger = true;
        }
    }
}
