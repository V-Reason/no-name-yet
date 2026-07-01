using UnityEngine;

namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 所有Checker的抽象基类
    /// </summary>
    public abstract class BaseChecker : MonoBehaviour, IChecker
    {
        [Header("每帧检测")]
        [Tooltip("是否允许 Detector 每帧自动调用 Check()")]
        public bool framCheck = true;
        // 存放Checker的检测结果
        public bool IsConditionMet { get; protected set; } = false;
        // 统一启动检测的接口
        public abstract void Check();
    }
}