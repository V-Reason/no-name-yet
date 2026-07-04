using UnityEngine;
using RPG2D.Core.Interaction;

namespace RPG2D.Item
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Anchor : MonoBehaviour, IGrabbable
    {
        public GrabType GrabType => GrabType.Static;
        public bool CanGrab() => true;
        public Transform GetTransform() => transform;

        private CircleCollider2D cld => GetComponent<CircleCollider2D>();

        private void Awake()
        {
            cld.isTrigger = true;
        }

        public Vector2 GetGrabPosition(Vector2 playerPosition)
        {
            // 锚点是固定的，直接返回自身位置
            return transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawIcon(transform.position, "Anchor_Icon", true);
        }
    }
}
