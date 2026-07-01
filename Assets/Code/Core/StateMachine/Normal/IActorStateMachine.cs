using RPG2D.Core.Actor;
using RPG2D.Core.AnimationEvent;
using RPG2D.Core.AnimatorWrapper;
using RPG2D.Core.Controller;
using RPG2D.Core.Data;
using RPG2D.Core.Detector;
using UnityEngine;

namespace RPG2D.Core.StateMachine
{
    public interface IActorStateMachine<TActorData> : IStateMachine, IAnimationEventTarget where TActorData : ActorData
    {
        TActorData actorData { get; }

        Rigidbody2D rb { get; }
        IController controller { get; }
        IActor actor { get; }
        IDetector detector { get; }
        IAnimatorWrapper animatorWrapper { get; }
    }
}
