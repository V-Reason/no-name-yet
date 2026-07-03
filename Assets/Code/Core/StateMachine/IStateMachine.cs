using UnityEngine;
using RPG2D.Core.Data;
using RPG2D.Core.State;

namespace RPG2D.Core.StateMachine
{
    /// <summary>
    /// 状态机接口
    /// </summary>
    public interface IStateMachine
    {
        void SwitchState<StateType>() where StateType : IState;
        void OnUpdate();
        void OnFixedUpdate();
    }
}