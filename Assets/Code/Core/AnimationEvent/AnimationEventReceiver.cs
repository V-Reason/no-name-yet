using System;
using UnityEngine;

namespace RPG2D.Core.AnimationEvent
{
    /// <summary>
    /// 动画事件接收器
    /// </summary>
    /// <remarks>
    /// 需要将该 接收器/子接收器 挂载在 有Animator的对象下
    /// </remarks>
    public abstract class AnimationEventReceiver : MonoBehaviour
    {
        // 需要转发的目标对象
        protected IAnimationEventTarget target;

        // 自底向上获取上层转发目标对象target
        protected virtual void Awake() => target = GetComponentInParent<IAnimationEventTarget>();

        // 动画可触发事件
        //......
        //举例：
        //public void EventA() => target.OnAnimationEvent(AnimationEventType.EventAAA);
    }

}