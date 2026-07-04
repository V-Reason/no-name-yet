using UnityEngine;

namespace RPG2D.Item
{
    /// <summary>
    /// 玩家持有中的磁铁道具, 负责按配置持续时间自动结束并销毁自身.
    /// </summary>
    public class MagnetHeldItem : MonoBehaviour
    {
        [Header("磁铁持续时间")]
        [SerializeField, Min(0f)] private float duration = 8f;

        private float elapsedTime;

        public float Duration => duration;
        public bool IsExpired { get; private set; }

        private void OnEnable()
        {
            elapsedTime = 0f;
            IsExpired = false;
        }

        private void Update()
        {
            TickLifetime(Time.deltaTime);
        }

        /// <summary>
        /// 设置磁铁持续时间, 供策划调参后的运行时覆盖或测试验证使用.
        /// </summary>
        public void SetDuration(float value)
        {
            duration = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 推进持有道具生命周期, 到达持续时间后标记过期并销毁道具实例.
        /// </summary>
        public void TickLifetime(float deltaTime)
        {
            if (IsExpired)
            {
                return;
            }

            elapsedTime += Mathf.Max(0f, deltaTime);
            if (elapsedTime < duration)
            {
                return;
            }

            IsExpired = true;
            DestroySelf();
        }

        /// <summary>
        /// 根据当前运行环境销毁道具实例, 避免编辑器测试中调用 Destroy 产生警告.
        /// </summary>
        private void DestroySelf()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
