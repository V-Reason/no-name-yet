using System;
using UnityEngine;

namespace RPG2D.Item
{
    /// <summary>
    /// 玩家单持有道具容器, 当前只记录磁铁的有无和使用生成逻辑.
    /// </summary>
    public class PlayerItemHolder : MonoBehaviour
    {
        [Header("持有道具挂点")]
        [SerializeField] private Transform itemHoldPoint;

        [Header("磁铁道具")]
        [SerializeField] private GameObject magnetHeldPrefab;
        [SerializeField] private bool startWithMagnet = true;
        [SerializeField] private bool hasMagnet;

        public event Action<bool> OnMagnetChanged;

        public bool HasMagnet => hasMagnet;

        private void Awake()
        {
            hasMagnet = startWithMagnet;
        }

        /// <summary>
        /// 配置磁铁使用时的生成位置和持有预制体, 便于运行时或测试环境注入引用.
        /// </summary>
        public void SetMagnetUseContext(Transform holdPoint, GameObject heldPrefab)
        {
            itemHoldPoint = holdPoint;
            magnetHeldPrefab = heldPrefab;
        }

        /// <summary>
        /// 尝试拾取磁铁; 玩家已持有磁铁时拒绝拾取, 避免超过单个持有上限.
        /// </summary>
        public bool TryPickupMagnet()
        {
            if (hasMagnet)
            {
                return false;
            }

            SetHasMagnet(true);
            return true;
        }

        /// <summary>
        /// 尝试使用磁铁; 成功时生成持有道具预制体并立即消耗当前磁铁.
        /// </summary>
        public bool TryUseMagnet()
        {
            if (!hasMagnet || itemHoldPoint == null || magnetHeldPrefab == null)
            {
                return false;
            }

            GameObject heldItem = Instantiate(magnetHeldPrefab, itemHoldPoint);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;

            SetHasMagnet(false);
            return true;
        }

        /// <summary>
        /// 统一更新磁铁持有状态, 仅在状态变化时通知 UI 等外部模块.
        /// </summary>
        private void SetHasMagnet(bool value)
        {
            if (hasMagnet == value)
            {
                return;
            }

            hasMagnet = value;
            OnMagnetChanged?.Invoke(hasMagnet);
        }
    }
}
