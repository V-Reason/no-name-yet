using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Item
{
    public class ChainHook : MonoBehaviour, IHookable
    {
        public Chain ownerChain;
        public float detectRadius = 0.5f; // 稍微调大一点点
        public LayerMask interactableLayer;

        private Vector2 lastPosition; // 记录上一帧的位置

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (ownerChain.isHooked) return;

            Vector2 currentPosition = transform.position;
            Vector2 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            Collider2D hitCollider = null; // 统一使用 Collider2D 接收结果

            if (distance > 0.01f)
            {
                // 扫掠检测：返回路径上碰到的第一个 RaycastHit2D
                RaycastHit2D rayHit = Physics2D.CircleCast(lastPosition, detectRadius, direction.normalized, distance, interactableLayer);
                hitCollider = rayHit.collider;
            }
            else
            {
                // 静止检测：直接返回圆圈内的 Collider2D
                hitCollider = Physics2D.OverlapCircle(currentPosition, detectRadius, interactableLayer);
            }

            if (hitCollider != null)
            {
                // 排除自己所在的链条
                if (hitCollider.transform.IsChildOf(ownerChain.transform))
                {
                    lastPosition = currentPosition;
                    return;
                }

                IHookable target = hitCollider.GetComponent<IHookable>();
                if (target != null && target.CanBeHooked())
                {
                    ownerChain.ConnectTo(target);
                    Debug.Log($"<color=green>钩子连接成功: {hitCollider.name}</color>");
                }

                if (target != null && target.CanBeHooked())
                {
                    if (hitCollider.transform.IsChildOf(ownerChain.transform)) return;
                    ownerChain.ConnectTo(target);
                    // 关键：告诉目标，是我钩了你
                    target.OnHooked(this);
                }
            }

            lastPosition = currentPosition;

        }

        // 实现 IHookable 接口
        public Vector2 GetHookAttachPosition() => transform.position;
        public void OnHooked(ChainHook incomingHook) { }
        public void OnUnhooked() { }
        public bool CanBeHooked() => ownerChain.isHooked;

        private void OnDrawGizmos()
        {
            Gizmos.color = (ownerChain != null && ownerChain.isHooked) ? Color.green : Color.yellow;
            // 画出检测范围
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}
