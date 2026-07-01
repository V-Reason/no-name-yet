using RPG2D.Core.AnimationEvent;
using RPG2D.Core.AnimatorWrapper;

namespace RPG2D.Core.State
{
    /// <summary>
    /// 各状态的接口，
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
    public interface IState
    {
        // 状态转移步骤
        void Enter();
        void OnFixedUpdate();
        void OnUpdate();
        void Exit();

        // 动画事件接收
        void OnAnimationEvent(AEType eventType);

        // 全局检测状态
        bool CheckGlobalTransitions();
    }
}