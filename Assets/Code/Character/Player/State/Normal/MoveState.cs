namespace RPG2D.Character.Player
{
    /// <summary>
    /// 移动状态
    /// </summary>
    public class MoveState : PlayerState
    {
        public MoveState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsMoving, true);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!stateMachine.detector.IsMoving)
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }
            if (stateMachine.detector.IsGrounded && stateMachine.controller.inputData.Jump)
            {
                stateMachine.SwitchState<JumpState>();
                return;
            }

            stateMachine.actor.Move();
        }
        public override void Exit()
        {
            base.Exit();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsMoving, false);
        }
    }
}
