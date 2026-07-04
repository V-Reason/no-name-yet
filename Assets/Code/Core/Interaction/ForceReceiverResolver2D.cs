using System.Collections.Generic;
using UnityEngine;

namespace RPG2D.Core.Interaction
{
    /// <summary>
    /// 将 2D 碰撞检测结果解析为唯一的 IForceReceiver 受力对象。
    /// </summary>
    public static class ForceReceiverResolver2D
    {
        /// <summary>
        /// 从命中的 Collider2D 向父级查找受力接口，并按同一物理帧的集合去重。
        /// </summary>
        public static bool TryGetUniqueReceiver(
            Collider2D targetCollider,
            ISet<IForceReceiver> appliedReceivers,
            out IForceReceiver receiver)
        {
            receiver = null;
            if (targetCollider == null)
            {
                return false;
            }

            receiver = targetCollider.GetComponentInParent<IForceReceiver>();
            if (receiver == null)
            {
                return false;
            }

            if (appliedReceivers != null && !appliedReceivers.Add(receiver))
            {
                receiver = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取用于计算力场强度的目标世界坐标，优先使用碰撞体边界中心。
        /// </summary>
        public static Vector2 GetForceSamplePosition(Collider2D targetCollider)
        {
            if (targetCollider == null)
            {
                return Vector2.zero;
            }

            return targetCollider.bounds.center;
        }
    }
}
