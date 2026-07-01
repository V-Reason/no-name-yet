using UnityEngine;
using RPG2D.Core.Data;
using RPG2D.Core.Actor;
using RPG2D.Core.AnimationEvent;
using RPG2D.Core.AnimatorWrapper;
using RPG2D.Core.Controller;
using RPG2D.Core.Detector;

namespace RPG2D.Core.StateMachine
{
    // 请将需要的组件写在这里
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class ActorStateMachine<TActorData>
        : BaseStateMachine, IActorStateMachine<TActorData>
        where TActorData : ActorData
    {
        [Header("必须手动设置引用")]
        [SerializeField] private TActorData _actorData;
        public TActorData actorData => _actorData;

        // 各组件的注册请写在SetupComponent()中给基类调用
        public Rigidbody2D rb { get; protected set; }
        public IController controller { get; protected set; }
        public IActor actor { get; protected set; }
        public IDetector detector { get; protected set; }
        public IAnimatorWrapper animatorWrapper { get; protected set; }

        // 必须覆写

        /// <summary>
        /// 自动注册各组件
        /// </summary>
        protected override void SetupComponent()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<IController>();
            detector = GetComponent<IDetector>();
            actor = GetComponent<IActor>();
            animatorWrapper = GetComponent<IAnimatorWrapper>();
        }
        /// <summary>
        /// 注册状态池，
        /// 需要手动把各状态依次放入状态池，
        /// 新加了状态后请手动添加在这
        /// </summary>
        protected override abstract void RegisterAllState();
        /// <summary>
        /// 指定初始状态
        /// </summary>
        protected override abstract void SwtichToInitialState();

        // 动画事件转发
        public void OnAnimationEvent(AEType animationEvent)
        {
            currentState?.OnAnimationEvent(animationEvent);
        }
    }
}
