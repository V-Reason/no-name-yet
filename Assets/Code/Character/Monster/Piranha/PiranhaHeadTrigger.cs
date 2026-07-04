using RPG2D.Character.Player;
using UnityEngine;

namespace RPG2D.Character.Monster.Piranha
{
    [RequireComponent(typeof(Collider2D))]
    public class PiranhaHeadTrigger : MonoBehaviour
    {
        [SerializeField] private PiranhaAI piranhaAI;

        private void Awake()
        {
            if (piranhaAI == null)
            {
                piranhaAI = GetComponentInParent<PiranhaAI>();
            }
        }

        private void Reset()
        {
            piranhaAI = GetComponentInParent<PiranhaAI>();
            EnsureTriggerCollider();
        }

        private void OnValidate()
        {
            EnsureTriggerCollider();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryForwardCapture(other);
        }

        /// <summary>
        /// 将头部触发区命中的玩家转交给食人鱼 AI 处理捕获。
        /// </summary>
        private void TryForwardCapture(Collider2D other)
        {
            if (piranhaAI == null || other == null || !other.CompareTag("Player"))
            {
                return;
            }

            StateMachine player = other.GetComponentInParent<StateMachine>();
            if (player == null)
            {
                return;
            }

            piranhaAI.TryCapture(player);
        }

        /// <summary>
        /// 确保头部碰撞体处于触发器模式，避免和捕获检测职责混在一起。
        /// </summary>
        private void EnsureTriggerCollider()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }
    }
}
