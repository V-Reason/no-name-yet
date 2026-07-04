using UnityEngine;
using RPG2D.Core.StateMachine;

namespace RPG2D.Character.Player
{
    // 将需要的组件写在这里
    [RequireComponent(typeof(Controller))]
    [RequireComponent(typeof(Detector))]
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(AnimatorWrapper))]
    public class StateMachine : ActorStateMachine<PlayerData>
    {
        // 覆写基类组件
        public new Controller controller => base.controller as Controller;
        public new Detector detector => base.detector as Detector;
        public new Actor actor => base.actor as Actor;
        public new AnimatorWrapper animatorWrapper => base.animatorWrapper as AnimatorWrapper;

        /// <summary>
        /// 注册状态池，
        /// 需要手动把各状态依次放入状态池，
        /// 新加了状态后请手动添加在这
        /// </summary>
        protected override void RegisterAllState()
        {
            // this表示把stateMachine注入各state
            // 使得各state可以调用stateMachine
            RegisterState(new IdleState(this));
            RegisterState(new MoveState(this));
            RegisterState(new ClimbState(this));
        }

        // 指定初始状态
        protected override void SwtichToInitialState()
        {
            SwitchState<IdleState>();
        }

        // 驱动接口
        public override void OnUpdate()
        {
            controller.OnUpdate();
            detector.OnUpdate();
            base.OnUpdate();
            animatorWrapper.OnUpdate(detector.checkData);

            UpdateStateValue();
        }

        // 驱动
        private void Update()
        {
            OnUpdate();
        }

        // 更新状态值
        private void UpdateStateValue()
        {
        }
    }
}
