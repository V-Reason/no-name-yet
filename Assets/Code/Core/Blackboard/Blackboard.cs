using UnityEngine;

public class Blackboard : Singleton<Blackboard>
{
    [Header("实时游戏数据")]
    public float currentDepth;
    public float targetDepth = 500f;

    [Header("磁铁状态")]
    public bool hasMagnet;
    public bool isMagnetActive;

    public void ResetData()
    {
        currentDepth = 0;
        hasMagnet = false;
        isMagnetActive = false;
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
