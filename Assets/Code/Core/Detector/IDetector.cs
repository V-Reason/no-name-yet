#nullable enable
using RPG2D.Core.Checker;
using UnityEngine;

namespace RPG2D.Core.Detector
{
    /// <summary>
    /// 环境检测器，
    /// 用于反映物理客观，拥有多个Checker，
    /// 整理Checkers的结果打包成CheckData对外反映物理客观
    /// </summary>
    public interface IDetector
    {
        CheckData checkData { get; }

        // 对外接口
        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定检测层的执行位次
        /// </summary>
        void OnUpdate();
        // 开始检测，多加了一层
        // Tick模式，需要放在update里
        void OnceDetect();
        // 指定检测一次
        bool OnceCheck<whichOne>() where whichOne : IChecker;
        // 泛型，T代表需要的Checker类型
        bool GetCondition<whichOne>() where whichOne : IChecker;
        // 获取单个Checker
        whichOne? GetChecker<whichOne>() where whichOne : class, IChecker;
    }
}