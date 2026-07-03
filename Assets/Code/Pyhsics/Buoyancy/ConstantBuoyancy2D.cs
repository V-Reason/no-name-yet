using UnityEngine;

namespace RPG2D.Pyhsics.Buoyancy
{
    /// <summary>
    /// 恒定 2D 浮力组件, 开启时持续给 Rigidbody2D 施加向上的力.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ConstantBuoyancy2D : MonoBehaviour, IBuoyancy2D
    {
        [SerializeField, Min(0f)]
        private float buoyancyForce = 10f;

        [SerializeField]
        private bool useWaterDrag = true;

        [SerializeField, Min(0f)]
        private float waterDrag = 3f;

        [SerializeField]
        private bool isBuoyancyEnabled = true;

        private Rigidbody2D rb;
        private float originalDrag;

        public bool IsBuoyancyEnabled => isBuoyancyEnabled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            originalDrag = rb.drag;
            ApplyDragState();
        }

        private void OnDisable()
        {
            RestoreOriginalDrag();
        }

        private void FixedUpdate()
        {
            if (!isBuoyancyEnabled || buoyancyForce <= 0f)
            {
                return;
            }

            rb.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
        }

        /// <summary>
        /// 开启浮力, 使组件在物理更新中继续施加向上的力.
        /// </summary>
        public void EnableBuoyancy()
        {
            SetBuoyancyEnabled(true);
        }

        /// <summary>
        /// 关闭浮力, 使组件停止继续施加向上的力.
        /// </summary>
        public void DisableBuoyancy()
        {
            SetBuoyancyEnabled(false);
        }

        /// <summary>
        /// 根据传入状态统一设置浮力开关.
        /// </summary>
        public void SetBuoyancyEnabled(bool isEnabled)
        {
            isBuoyancyEnabled = isEnabled;
            ApplyDragState();
        }

        /// <summary>
        /// 根据浮力开关同步水中阻力, 开启时增加阻力, 关闭时恢复原始阻力.
        /// </summary>
        private void ApplyDragState()
        {
            if (rb == null)
            {
                return;
            }

            rb.drag = isBuoyancyEnabled && useWaterDrag ? waterDrag : originalDrag;
        }

        /// <summary>
        /// 组件停用时恢复 Rigidbody2D 原始阻力, 避免残留水中阻力设置.
        /// </summary>
        private void RestoreOriginalDrag()
        {
            if (rb == null)
            {
                return;
            }

            rb.drag = originalDrag;
        }
    }
}
