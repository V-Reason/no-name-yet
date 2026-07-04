using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Item
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class ChainHook : MonoBehaviour, IHookable
    {
        public Chain ownerChain; // 拖入父级的 Chain 脚本
        public LayerMask hookableMask; // 设置为包含 Anchor 和其他 Hook 的层级

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (ownerChain.isHooked) return; // 已经钩住了就不用再检测

            // 尝试获取 IHookable 接口
            IHookable target = other.GetComponent<IHookable>();

            // 确保不是钩到自己
            if (target != null && (MonoBehaviour)target != this && target.CanBeHooked())
            {
                ownerChain.ConnectTo(target);
                Debug.Log($"钩住了: {other.name}");
            }
        }

        // 实现 IHookable 接口，让链条可以互钩
        public Vector2 GetHookAttachPosition() => transform.position;
        public void OnHooked(ChainHook incomingHook) { }
        public void OnUnhooked() { }
        public bool CanBeHooked() => ownerChain.isHooked; // 只有当我自己处于固定状态，别人才能钩我
    }
}
