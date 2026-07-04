namespace RPG2D.Character.Player
{
    public class IdleState : PlayerState
    {
        public IdleState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (stateMachine.detector.IsMoving)
            {
                stateMachine.SwitchState<MoveState>();
                return;
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
        }
    }
}
