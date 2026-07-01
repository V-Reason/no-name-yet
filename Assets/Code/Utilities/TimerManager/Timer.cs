using System;
using UnityEngine;

namespace RPG2D.Utilities
{
    /// <summary>
    /// 计时器工具
    /// 以秒为单位
    /// </summary>
    /// <remarks>
    /// 默认构造时，RemainingTime == Duration == 0f
    /// 且 IsRunning == false，IsFinished == true
    /// </remarks>
    [System.Serializable]
    public class Timer
    {
        // 剩余时间
        public float RemainingTime { get; private set; } = 0f;
        // 总时间
        public float Duration { get; private set; } = 0f;
        // 是否运行 = 启动过&&正在计时
        public bool IsRunning => Duration > 0 && RemainingTime > 0;
        // 是否结束 = 无剩余时间
        public bool IsFinished => RemainingTime <= 0;

        // 进度条（0~1）
        public float Progress => Duration > 0 ? Mathf.Clamp01(1 - RemainingTime / Duration) : 0;

        // 对外接口
        //开始计时，duration为总时间
        public void Start(float duration)
        {
            Duration = duration;
            RemainingTime = duration;
            // LOG($"计时器START ({Duration}s)");
        }

        // 对外接口
        //Tick模式，deltaTime为时间变化值，需要自行安排到Update()中
        public void Tick(float deltaTime)
        {
            if (RemainingTime > 0)
            {
                RemainingTime -= deltaTime;
                if (RemainingTime <= 0) // 不要把计时器停止放到下一帧，会出bug
                    Stop();
            }
        }

        // 对外接口
        //重置计时器，仅把原有的持续时间重置
        public void Reset()
        {
            if (Duration <= 0)
            {
                // LOG_WARNING("计时器重置失败:Duration为0,请先调用Start");
                return;
            }
            RemainingTime = Duration;
        }

        // 对外接口
        //停止计时，关闭计时器
        public void Stop()
        {
            // LOG($"计时器OK ({Duration}s)");
            RemainingTime = 0;
            Duration = 0;
        }
    }
}