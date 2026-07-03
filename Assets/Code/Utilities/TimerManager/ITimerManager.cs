using System;

namespace RPG2D.Utilities
{
    public interface ITimerManager<TEnum> where TEnum : Enum
    {
        void StartTimer(TEnum whichOne, float duration);
        void ResetTimer(TEnum whichOne);
        void StopTimer(TEnum whichOne);
        bool IsTimeUp(TEnum whichOne);
        bool IsRunning(TEnum whichOne);
        void TickAllTimers();
        Timer GetTimer(TEnum whichOne);
    }
}