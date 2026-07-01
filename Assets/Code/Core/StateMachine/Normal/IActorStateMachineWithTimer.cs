using System;
using RPG2D.Utilities;

namespace RPG2D.Core.StateMachine
{
    public interface IActorStateMachineWithTimer<TActorData, TActorSkill>
        : IActorStateMachine<TActorData>, ITimerManager<TActorSkill>
        where TActorData : RPG2D.Core.Data.ActorData
        where TActorSkill : Enum
    {
    }
}