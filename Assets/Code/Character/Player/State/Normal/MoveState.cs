namespace RPG2D.Character.Player
{
    public class MoveState : PlayerState
    {
        public MoveState(StateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsSwimming, true);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!stateMachine.detector.IsMoving)
            {
                stateMachine.SwitchState<IdleState>();
                return;
            }
            if (stateMachine.detector.checkData.CanGrab && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.E))
            {
                stateMachine.SwitchState<ClimbState>();
                return;
            }

            stateMachine.actor.Move();
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.animatorWrapper.SetBool(stateMachine.animatorWrapper.IsSwimming, false);
        }
    }
}
