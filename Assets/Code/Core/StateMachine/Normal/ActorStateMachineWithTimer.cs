using System;
using RPG2D.Utilities;

namespace RPG2D.Core.StateMachine
{
    /// <summary>
    /// 角色状态机，
    /// </summary>
    /// <remarks>
    /// 最上层的中央组件，负责统一调度所有的组件，
    /// 通过向各状态注入的方式，使得各状态可以通过状态机间接获取各组件参数。
    /// 总依赖方式是自顶向下依赖，即状态机依赖各组件，各组件不依赖状态机。
    /// 必要时，需要使用依赖倒置原则，比如IAnimationEventTarget的方案
    /// </remarks>

    public abstract class ActorStateMachineWithTimer<TActorData, TActorSkill>
    : ActorStateMachine<TActorData>, IActorStateMachineWithTimer<TActorData, TActorSkill>
        where TActorData : RPG2D.Core.Data.ActorData
        where TActorSkill : Enum
    {
        // 计时器池
        protected TimerManager<TActorSkill> tm = new();

        public override void OnUpdate()
        {
            base.OnUpdate();
            // 驱动计时
            tm.TickAllTimers();
        }

        // 计时器转发
        public void StartTimer(TActorSkill whichOne, float duration) => tm.StartTimer(whichOne, duration);
        public void ResetTimer(TActorSkill whichOne) => tm.ResetTimer(whichOne);
        public void StopTimer(TActorSkill whichOne) => tm.StopTimer(whichOne);
        public bool IsTimeUp(TActorSkill whichOne) => tm.IsTimeUp(whichOne);
        public bool IsRunning(TActorSkill whichOne) => tm.IsRunning(whichOne);
        public void TickAllTimers() { return; } // 封闭对外的Tick接口
        public Timer GetTimer(TActorSkill whichOne) => tm.GetTimer(whichOne);
    }
}