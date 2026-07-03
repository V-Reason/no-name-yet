using System;
using System.Collections.Generic;

namespace RPG2D.Utilities
{
    public class TimerManager<TEnum> : ITimerManager<TEnum> where TEnum : Enum
    {
        private readonly Dictionary<TEnum, Timer> timers = new();

        // 自动注册计时器
        public TimerManager()
        {
            foreach (TEnum whichOne in Enum.GetValues(typeof(TEnum)))
            {
                timers[whichOne] = new Timer();
            }
        }

        // 计时器启动
        public void StartTimer(TEnum whichOne, float duration)
        {
            if (timers.TryGetValue(whichOne, out var timer))
            {
                timer.Start(duration);
            }
        }

        // 计时器重置
        public void ResetTimer(TEnum whichOne)
        {
            if (timers.TryGetValue(whichOne, out var timer))
            {
                timer.Reset();
            }
        }

        // 计时器终止
        public void StopTimer(TEnum whichOne)
        {
            if (timers.TryGetValue(whichOne, out var timer))
            {
                timer.Stop();
            }
        }

        // 驱动所有计时器
        public void TickAllTimers()
        {
            foreach (var timer in timers.Values)
            {
                // 只驱动正在运行的，避免性能浪费
                if (timer.IsRunning) timer.Tick(UnityEngine.Time.deltaTime);
            }
        }

        // 计时器查询
        public bool IsTimeUp(TEnum whichOne)
        {
            return timers.ContainsKey(whichOne) && timers[whichOne].IsFinished;
        }
        public bool IsRunning(TEnum whichOne)
        {
            return timers.ContainsKey(whichOne) && timers[whichOne].IsRunning;
        }

        // 获取计时器
        public Timer GetTimer(TEnum whichOne)
        {
            if (timers.TryGetValue(whichOne, out var timer))
            {
                return timer;
            }
            return null;
        }
    }
}