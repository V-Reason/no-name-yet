using System;

namespace RPG2D.Utilities
{
    /// <summary>
    /// 计数器
    /// 以次为单位,默认正向计数,计数范围从 0 到 totalCount
    /// </summary>
    /// <remarks>
    /// 默认构造时，currentCount == totalCount == 0，step == 1
    /// 且 IsEmpty == IsFull == true，CanIncrement == CanDecrement == false
    /// </remarks>
    [System.Serializable]
    public class Counter
    {
        // 当前计数
        public int currentCount { get; private set; } = 0;
        // 总计数
        public int totalCount { get; private set; } = 0;
        // 计数步长
        public int step { get; private set; } = 1;

        // 计数器状态
        public bool IsEmpty => currentCount == 0;
        public bool IsFull => currentCount + step > totalCount; // 修改step后可能使得IsFull状态改变
        public float Progress => totalCount > 0 ? (float)currentCount / totalCount : 0;

        public bool CanIncrement => currentCount + step <= totalCount;
        public bool CanDecrement => currentCount - step >= 0;

        // 对外接口
        //启动计数
        public void Start(int totalCount, int step = 1, int currentCount = 0)
        {
            if (totalCount <= 0) throw new ArgumentException("Counter中totalCount必须为正数", nameof(totalCount));
            else if (step <= 0) throw new ArgumentException("Counter中step必须为正数", nameof(step));
            else if (currentCount < 0) throw new ArgumentException("Counter中currentCount必须为零或正整数", nameof(currentCount));
            else if (currentCount > totalCount) throw new ArgumentException("Counter中currentCount必须小于等于totalCount", nameof(currentCount));

            this.totalCount = totalCount;
            this.currentCount = currentCount;
            this.step = step;
        }

        //设置步长
        public void SetStep(int value)
        {
            if (value <= 0) throw new ArgumentException("Counter中step必须为整数", nameof(value));
            this.step = value;
        }

        //设置当前计数
        public void SetCurrentCount(int value)
        {
            if (value < 0) throw new ArgumentException("Counter中currentCount必须为零或正整数", nameof(value));
            else if (value > totalCount) throw new ArgumentException("Counter中currentCount必须小于等于totalCount", nameof(value));
            this.currentCount = value;
        }

        //向上计数（wrapAround表示是否回绕）
        public bool Increment(bool wrapAround = false)
        {
            currentCount += step;
            if (currentCount > totalCount)
            {
                if (wrapAround)
                {
                    currentCount %= totalCount;
                }
                else
                {
                    currentCount -= step;
                    return false;
                }
            }
            return true;
        }

        //向下计数（wrapAround表示是否回绕）
        public bool Decrement(bool wrapAround = false)
        {
            currentCount -= step;
            if (currentCount < 0)
            {
                if (wrapAround)
                {
                    currentCount = (currentCount + totalCount) % totalCount;
                }
                else
                {
                    currentCount += step;
                    return false;
                }
            }
            return true;
        }

        //重置计数
        public void Reset() => currentCount = 0;

        //关闭计数
        public void Stop() => currentCount = totalCount = 0;
    }
}