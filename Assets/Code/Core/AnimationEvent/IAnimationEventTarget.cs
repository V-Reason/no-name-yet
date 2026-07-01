namespace RPG2D.Core.AnimationEvent
{
    /// <summary>
    /// 接口定义
    /// 动画事件转发目标接口
    /// </summary>
    /// <remarks>
    /// 依赖倒置原则
    /// 因为AnimationEventReceiver需要自底向上通知状态机StateMachine，
    /// 为了解耦，避免AnimationEventReceiver依赖StateMachine，
    /// 此处定义该接口，使用依赖倒置原则，
    /// 故需要StateMachine继承该接口，然后实现接口的通讯规则，保证正确转发到各状态state
    /// Tip: Target表示Receiver需要转发到的目标对象
    /// </remarks>
    public interface IAnimationEventTarget
    {
        void OnAnimationEvent(AEType eventType);
    }

}