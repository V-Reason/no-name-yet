using UnityEngine;

namespace RPG2D.Item
{
    /// <summary>
    /// 场景磁铁拾取物, 玩家触发后尝试把磁铁放入单持有道具槽.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MagnetPickupItem : MonoBehaviour
    {
        private void Reset()
        {
            Collider2D pickupCollider = GetComponent<Collider2D>();
            if (pickupCollider != null)
            {
                pickupCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (TryPickupFrom(other))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 尝试从触发碰撞体所属玩家身上获取道具容器, 成功时写入磁铁持有状态.
        /// </summary>
        public bool TryPickupFrom(Collider2D other)
        {
            if (other == null)
            {
                return false;
            }

            PlayerItemHolder holder = other.GetComponent<PlayerItemHolder>();
            return holder != null && holder.TryPickupMagnet();
        }
    }
}
