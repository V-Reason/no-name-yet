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
        public float reconnectDelay = 1f;
        private float cooldownTimer;

        private CircleCollider2D myCollider;
        private Vector2 lastPosition;

        private void Awake()
        {
            myCollider = GetComponent<CircleCollider2D>();
            myCollider.isTrigger = true;
            myCollider.radius = detectRadius;
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
                lastPosition = transform.position;
                return;
            }
            if (hookedTarget != null) return;

            Vector2 currentPosition = transform.position;
            Vector2 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            if (distance > 0.01f)
            {
                RaycastHit2D[] rayHits = Physics2D.CircleCastAll(lastPosition, detectRadius, direction.normalized, distance, interactableLayer);
                foreach (var rayHit in rayHits)
                {
                    if (TryConnect(rayHit.collider)) break;
                }
            }
            else
            {
                DetectPotentialTargets();
            }

            lastPosition = currentPosition;
        }

        private void DetectPotentialTargets()
        {
            if (cooldownTimer > 0) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, interactableLayer);

            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(ownerChain.transform)) continue;
                if (ownerChain.parentAnchor != null && hit.transform == ownerChain.parentAnchor.transform) continue;

                IHookable target = hit.GetComponent<IHookable>();
                if (target != null && target.CanBeHooked())
                {
                    ConnectTo(target);
                    break;
                }
            }
        }

        private bool TryConnect(Collider2D hitCollider)
        {
            if (hitCollider == myCollider) return false;
            if (hitCollider.transform.IsChildOf(ownerChain.transform)) return false;
            if (ownerChain.parentAnchor != null && hitCollider.transform == ownerChain.parentAnchor.transform) return false;

            IHookable target = hitCollider.GetComponent<IHookable>();
            if (target != null && target.CanBeHooked())
            {
                ConnectTo(target);
                return true;
            }
            return false;
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
                Vector2 pushDir = ((Vector2)transform.position - hookedTarget.GetHookAttachPosition()).normalized;
                if (pushDir == Vector2.zero) pushDir = Vector2.up;

                hookedTarget.OnUnhooked();
                hookedTarget = null;

                ownerChain.ApplyRawImpulse(pushDir * 5f);

                cooldownTimer = reconnectDelay;
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
