using System.Collections.Generic;
using RPG2D.Character.Monster.Piranha;
using RPG2D.Pyhsics.Buoyancy;
using UnityEngine;

namespace RPG2D.Character.Player
{
    public class PiranhaCapturedState : PlayerState
    {
        private PiranhaAI capturingPiranha;
        private Transform capturePoint;
        private IBuoyancy2D buoyancy;
        private bool originalIsKinematic;
        private Vector2 releaseImpulse;
        private readonly List<ColliderPair> ignoredCollisionPairs = new();

        public PiranhaCapturedState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            capturingPiranha = stateMachine.CapturingPiranha;
            capturePoint = stateMachine.PiranhaCapturePoint;
            buoyancy = stateMachine.GetComponent<IBuoyancy2D>();
            originalIsKinematic = stateMachine.rb.isKinematic;
            releaseImpulse = capturingPiranha != null ? capturingPiranha.ReleaseImpulse : Vector2.up;

            stateMachine.rb.velocity = Vector2.zero;
            stateMachine.rb.isKinematic = true;
            buoyancy?.DisableBuoyancy();
            SetPiranhaCollisionIgnored(true);
            SnapToCapturePoint();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (capturingPiranha == null || capturePoint == null)
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }

            SnapToCapturePoint();

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            {
                capturingPiranha.ReleaseCapturedPlayer();
                stateMachine.SwitchState<IdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.rb.isKinematic = originalIsKinematic;
            buoyancy?.EnableBuoyancy();
            stateMachine.rb.velocity = releaseImpulse;
            SetPiranhaCollisionIgnored(false);
            stateMachine.ClearPiranhaCapture();

            capturingPiranha = null;
            capturePoint = null;
            buoyancy = null;
        }

        /// <summary>
        /// 将玩家位置同步到食人鱼咬合点，保证被捕获期间持续跟随移动。
        /// </summary>
        private void SnapToCapturePoint()
        {
            if (capturePoint != null)
            {
                stateMachine.transform.position = capturePoint.position;
            }
        }

        /// <summary>
        /// 捕获期间忽略玩家和食人鱼本体碰撞，避免重叠时物理求解把食人鱼顶飞。
        /// </summary>
        private void SetPiranhaCollisionIgnored(bool ignored)
        {
            if (!ignored)
            {
                RestoreIgnoredCollisionPairs();
                return;
            }

            ignoredCollisionPairs.Clear();
            if (capturingPiranha == null)
            {
                return;
            }

            Collider2D[] playerColliders = stateMachine.GetComponentsInChildren<Collider2D>();
            Collider2D[] piranhaColliders = capturingPiranha.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null || !playerCollider.enabled || playerCollider.isTrigger)
                {
                    continue;
                }

                foreach (Collider2D piranhaCollider in piranhaColliders)
                {
                    if (piranhaCollider == null || !piranhaCollider.enabled || piranhaCollider.isTrigger)
                    {
                        continue;
                    }

                    Physics2D.IgnoreCollision(playerCollider, piranhaCollider, true);
                    ignoredCollisionPairs.Add(new ColliderPair(playerCollider, piranhaCollider));
                }
            }
        }

        /// <summary>
        /// 退出捕获状态时恢复进入状态时临时忽略的碰撞关系。
        /// </summary>
        private void RestoreIgnoredCollisionPairs()
        {
            foreach (ColliderPair pair in ignoredCollisionPairs)
            {
                if (pair.PlayerCollider != null && pair.PiranhaCollider != null)
                {
                    Physics2D.IgnoreCollision(pair.PlayerCollider, pair.PiranhaCollider, false);
                }
            }

            ignoredCollisionPairs.Clear();
        }

        private readonly struct ColliderPair
        {
            public ColliderPair(Collider2D playerCollider, Collider2D piranhaCollider)
            {
                PlayerCollider = playerCollider;
                PiranhaCollider = piranhaCollider;
            }

            public Collider2D PlayerCollider { get; }
            public Collider2D PiranhaCollider { get; }
        }
    }
}
