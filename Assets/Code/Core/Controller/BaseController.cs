using Unity.VisualScripting;
using UnityEngine;

namespace RPG2D.Core.Controller
{
    /// <summary>
    /// 控制信息处理器，
    /// 用于接收控制信息，标准化输入参数，
    /// 监测整理输入结果打包成InputData对外反映玩家控制
    /// </summary>
    public abstract class BaseController<TInputData> : MonoBehaviour, IController where TInputData : InputData, new()
    {
        // 对外参数
        public TInputData inputData { get; protected set; }
        InputData IController.inputData => inputData;
        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定输入监测层的执行时机
        /// </summary>
        public abstract void OnUpdate();

        protected void Awake()
        {
            inputData = new TInputData();
        }
    }
}