using System;
using System.Collections.Generic;

/// <summary>
/// 游戏事件枚举
/// </summary>
public enum EventType
{
    GameStateChanged,
}

public static class EventCenter
{
    private static Dictionary<EventType, Action<object>> eventTable = new Dictionary<EventType, Action<object>>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    public static void Subscribe(EventType eventType, Action<object> callback)
    {
        if (!eventTable.ContainsKey(eventType))
            eventTable.Add(eventType, null);
        eventTable[eventType] += callback;
    }

    /// <summary>
    /// 解除订阅
    /// </summary>
    public static void Unsubscribe(EventType eventType, Action<object> callback)
    {
        if (eventTable.ContainsKey(eventType))
            eventTable[eventType] -= callback;
    }

    /// <summary>
    /// 广播事件
    /// </summary>
    public static void Broadcast(EventType eventType, object data = null)
    {
        if (eventTable.TryGetValue(eventType, out Action<object> callback))
        {
            callback?.Invoke(data);
        }
    }
}

// 注意：
// 请再OnEnable()和OnDisable()中配套使用，避免切换场景时的空引用
// 示例：
// void OnEnable()
// {
//     EventCenter.Subscribe(EventType.OnGameStateChanged, HandleStateUI);
// }

// void OnDisable()
// {
//     EventCenter.Unsubscribe(EventType.OnGameStateChanged, HandleStateUI);
// }
