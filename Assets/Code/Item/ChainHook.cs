using RPG2D.Core.Interaction;
using UnityEngine;

namespace RPG2D.Item
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class ChainHook : MonoBehaviour, IHookable
    {
        public Chain ownerChain;
        public IHookable hookedTarget;
        public ChainHook incomingHook;

        public float detectRadius = 0.4f;
        public LayerMask interactableLayer;

        private Vector2 lastPosition;



        private void Awake()
        {
            var cld = GetComponent<CircleCollider2D>();
            cld.isTrigger = true;
            cld.radius = detectRadius;
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (hookedTarget != null) return;

            Vector2 currentPosition = transform.position;
            Vector2 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            Collider2D hitCollider = null;

            if (distance > 0.01f)
            {
                RaycastHit2D rayHit = Physics2D.CircleCast(lastPosition, detectRadius, direction.normalized, distance, interactableLayer);
                hitCollider = rayHit.collider;
            }
            else
            {
                hitCollider = Physics2D.OverlapCircle(currentPosition, detectRadius, interactableLayer);
            }

            if (hitCollider != null)
            {
                if (hitCollider.transform.IsChildOf(ownerChain.transform))
                {
                    lastPosition = currentPosition;
                    return;
                }

                IHookable target = hitCollider.GetComponent<IHookable>();
                if (target != null && target.CanBeHooked())
                {
                    ConnectTo(target);
                    Debug.Log($"<color=green>钩子连接成功: {hitCollider.name}</color>");
                }
            }

            lastPosition = currentPosition;
        }

        public void ConnectTo(IHookable target)
        {
            hookedTarget = target;
            target.OnHooked(this);
            if (ownerChain != null) ownerChain.isHooked = true;
            Debug.Log($"{ownerChain.name} 钩住了 {((MonoBehaviour)target).name}");
        }

        public void Disconnect()
        {
            if (hookedTarget != null)
            {
                hookedTarget.OnUnhooked();
                hookedTarget = null;
                if (ownerChain != null) ownerChain.isHooked = false;
            }
        }

        // IHookable 实现（允许被另一个钩子钩住）
        public Vector2 GetHookAttachPosition() => transform.position;
        public bool CanBeHooked() => incomingHook == null;
        public void OnHooked(ChainHook hook) => incomingHook = hook;
        public void OnUnhooked() => incomingHook = null;
        public Chain GetRelatedChain() => ownerChain;
        public HookPointType GetHookPointType() => HookPointType.Tail;

        private void OnDrawGizmos()
        {
            Gizmos.color = hookedTarget != null ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}
