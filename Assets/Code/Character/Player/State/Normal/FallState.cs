using UnityEngine.UIElements;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 下落状态
    /// </summary>
    public class FallState : PlayerState
    {
        public FallState(StateMachine stateMachine) : base(stateMachine) { }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (stateMachine.detector.IsGrounded)
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }

            // 下落保持移动
            stateMachine.actor.Move();
        }
    }
}
