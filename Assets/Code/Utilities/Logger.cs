using UnityEngine;

namespace RPG2D.Utilities
{
    /// <summary>
    /// 日志工具类
    /// </summary>
    /// <code>
    /// using static Logger;
    /// LOG($"Current Value: {value}");
    /// LOG_WARNING("Warning!");
    /// LOG_ERROR("Error!");
    /// </code>
    // 静态类 + Conditional 特性
    public static class Logger
    {
        // 只有定义了 ENABLE_LOG 宏，这个方法的调用才参与编译
        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public static void LOG(object message) => Debug.Log(message);

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public static void LOG_WARNING(object message) => Debug.LogWarning(message);

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public static void LOG_ERROR(object message) => Debug.LogError(message);
    }
}