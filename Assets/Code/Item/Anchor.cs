using UnityEngine;
using RPG2D.Core.Interaction;

namespace RPG2D.Item
{
    [ExecuteAlways]
    public class Anchor : MonoBehaviour, IGrabbable, IHookable
    {
        public Chain attachedChain; // 该锚点向下连接的链条
        public ChainHook incomingHook; // 当前钩在此锚点上的钩子

        [Header("视觉表现")]
        public Sprite[] anchorSprites;
        public int selectedSpriteIndex;
        public int sortingOrder = 2;

        private SpriteRenderer spriteRenderer;

        public GrabType GrabType => GrabType.Static;
        public bool CanGrab() => true;
        public Transform GetTransform() => transform;
        public Vector2 GetGrabPosition(Vector2 playerPos) => transform.position;

        // IHookable 实现
        public Vector2 GetHookAttachPosition() => transform.position;
        public bool CanBeHooked() => incomingHook == null;
        public void OnHooked(ChainHook hook) => incomingHook = hook;
        public void OnUnhooked() => incomingHook = null;
        public Chain GetRelatedChain() => attachedChain;
        public HookPointType GetHookPointType() => HookPointType.Head;

        private void Awake()
        {
            incomingHook = null;
            if (attachedChain == null) attachedChain = GetComponentInChildren<Chain>();
            SetupSpriteRenderer();
        }

        private void SetupSpriteRenderer()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (anchorSprites != null && selectedSpriteIndex >= 0 && selectedSpriteIndex < anchorSprites.Length)
                spriteRenderer.sprite = anchorSprites[selectedSpriteIndex];

            spriteRenderer.sortingOrder = sortingOrder;
        }

        private void OnValidate()
        {
            SetupSpriteRenderer();
        }
    }
}
