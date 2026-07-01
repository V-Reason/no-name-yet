using System;
using System.Collections.Generic;
using UnityEngine;
using RPG2D.Core.State;

namespace RPG2D.Core.StateMachine
{
    /// <summary>
    /// 状态机抽象基类
    /// </summary>
    public abstract class BaseStateMachine : MonoBehaviour, IStateMachine
    {
#if UNITY_EDITOR
        [Header("调试信息")]
        [SerializeField, Unity.Collections.ReadOnly] private string currentStateName;   // 当前状态的名字
#endif
        // 内部参数
        //状态池
        private Dictionary<System.Type, IState> statePool = new();
        protected IState currentState;  // 标记当前状态
        private IState nextState;       // 标记即将切换的状态

        // 对外接口

        // 延迟切换状态，先标记，下一帧再转换，防止递归栈泄露
        // 泛型模式，T表示要切换到的状态
        public void SwitchState<StateType>() where StateType : IState
        {
            System.Type targetType = typeof(StateType);

            // [ 弃用规则，不适用于AttackState的设计 ]
            // // 无视自我转换
            // if (currentState != null && currentState.GetType() == targetType) return;

            // 无视已经在nextState里的状态
            if (nextState != null && nextState.GetType() == targetType) return;

            // 无视不在状态池里的状态
            if (statePool.TryGetValue(targetType, out var newState))
            {
                nextState = newState; // 标记状态
            }
        }

        // 内部核心

        // 自注册状态与组件
        protected virtual void Awake()
        {
            SetupComponent();
            RegisterAllState();
            SwtichToInitialState();
        }

        // 驱动接口
        public virtual void OnUpdate()
        {
            // 全局检测
            currentState?.CheckGlobalTransitions();

            // 允许同帧切换多个状态，优化状态切换手感，避免状态切换时顿帧
            int safetyCnt = 0; // 安全阈值，避免一直切换一直卡帧
            while (safetyCnt < 5)
            {
                if (nextState != null) PerformTransition();
                currentState?.OnUpdate();
                if (nextState == null) break;
                ++safetyCnt;
            }
        }
        // 物理驱动接口
        public virtual void OnFixedUpdate() { }

        // 正式切换状态
        private void PerformTransition()
        {
            // 先退出当前状态，再进入新状态
            currentState?.Exit();
            currentState = nextState;
            nextState = null; // 清空
            currentState?.Enter();
#if UNITY_EDITOR
            currentStateName = currentState?.GetType().Name;
#endif
        }

        // 注册单个状态
        protected void RegisterState(IState state)
            => statePool[state.GetType()] = state;

        // 必须覆写

        protected abstract void SetupComponent();
        /// <summary>
        /// 注册状态池，
        /// 需要手动把各状态依次放入状态池，
        /// 新加了状态后请手动添加在这
        /// </summary>
        protected abstract void RegisterAllState();
        /// <summary>
        /// 指定初始状态
        /// </summary>
        protected abstract void SwtichToInitialState();
    }
}