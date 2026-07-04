namespace RPG2D.Character.Player
{
    /// <summary>
    /// 待机状态
    /// </summary>
    public class IdleState : PlayerState
    {
        public IdleState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsIdle, true);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (stateMachine.detector.IsMoving)
            {
                stateMachine.SwitchState<MoveState>();
                return; // 立即切断逻辑！！！
            }
            if (stateMachine.detector.checkData.CanGrab && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.E))
            {
                stateMachine.SwitchState<ClimbState>();
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsIdle, false);
        }
    }
}
