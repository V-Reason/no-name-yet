namespace RPG2D.Character.Player
{
    /// <summary>
    /// 跳跃状态
    /// </summary>
    /// <remarks>
    /// 跳跃状态和下落状态是分开的
    /// </remarks>
    public class JumpState : PlayerState
    {
        public JumpState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            stateMachine.actor.Jump();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            // 防止跳跃卡顿，IsGrounded交给FallState
            // if (stateMachine.detector.IsGrounded)
            // {
            //     stateMachine.SwitchState<PlayerIdleState>();
            //     return;
            // }
            if (!stateMachine.detector.IsGrounded && stateMachine.rb.velocity.y < 0)
            {
                stateMachine.SwitchState<FallState>();
                return;
            }

            // 跳跃过程保持移动
            stateMachine.actor.Move();
        }
    }
}