using RPG2D.Core.Data;
using RPG2D.Core.State;

namespace RPG2D.Character.Player
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
    public abstract class PlayerState : BaseState<StateMachine>
    {
        // 构造时必须注入状态机，注意子类需要多写明一个构造函数
        // eg: public PlayerIdleState(PlayerStateMachine stateMachine):base(stateMacine){}
        protected PlayerState(StateMachine stateMachine) : base(stateMachine) { }

        // 全局检测状态，非虚！
        // 注意：由本层处理检测结果的逻辑，不要放到子检测层里
        public override bool CheckGlobalTransitions()
        {
            return false;
        }

        // 子检测，允许override以禁用该检测
        // 注意：本层不处理检测结果的逻辑，仅用于反映是否符合规则
    }
}
