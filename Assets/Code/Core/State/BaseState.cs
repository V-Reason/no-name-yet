using RPG2D.Core.AnimationEvent;
using RPG2D.Core.StateMachine;

namespace RPG2D.Core.State
{
    /// <summary>
    /// 各状态的抽象基类，
    /// 规范对外接口，
    /// 可作为AnyState状态，
    /// 定义全局检测函数
    /// </summary>
    /// <remarks>
    /// 各状态需要负责：
    /// 1. 状态切换条件判断
    /// 2. 执行切换状态逻辑
    /// 3. 执行动作逻辑
    /// 3. 控制行为组件
    /// 4. 控制动画组件
    /// 注意：
    ///     1. PlayerState是非Mono类，不要写Awake()，初始化请使用构造函数
    ///     2. 切换状态之后需要立即return打断逻辑，避免状态串味
    /// </remarks>
    public abstract class BaseState<TStateMachine> : IState
        where TStateMachine : IStateMachine
    {
        // 需要注入的状态机
        protected TStateMachine stateMachine;

        // 构造时必须注入状态机，注意子类需要多写明一个构造函数
        // eg: public PlayerIdleState(PlayerStateMachine stateMachine):base(stateMacine){}
        protected BaseState(TStateMachine stateMachine)
            => this.stateMachine = stateMachine;

        // 对外统一接口
        // 状态转移步骤
        public virtual void Enter() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void Exit() { }

        // 动画事件接收
        public virtual void OnAnimationEvent(AEType eventType) { }

        // 全局检测状态
        // 注意：由本层处理检测结果的逻辑，不要放到子检测层里
        public virtual bool CheckGlobalTransitions() => false;
    }
}