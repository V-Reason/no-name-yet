using UnityEngine;

public class Blackboard : Singleton<Blackboard>
{
    // [Header("配置文件")]
    // 拖入 ScriptableObject 配置文件
    // public PlayerData playerConfig;

    // [Header("实时数据")]

    // 重置数据
    public void ResetData()
    {
    }

    // 演示
    // 封装修改数据的逻辑，并自动触发广播
    // public void AddScore(int amount)
    // {
    //     currentScore += amount;
    //     EventCenter.Broadcast(EventType.OnScoreChanged, currentScore);
    // }

    // public void UpdateHealth(float newHealth)
    // {
    //     currentHealth = newHealth;
    //     EventCenter.Broadcast(EventType.OnHealthChanged, currentHealth);
    // }
}
