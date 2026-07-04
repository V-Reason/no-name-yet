using UnityEngine;

namespace RPG2D.Core.Interaction
{
    public enum GrabType
    {
        Static,     // 静态点（如锚点），玩家固定不动
        Linear,     // 线性路径（如链条），玩家可以上下爬行
        Free        // 自由附着（如大型鱼类），玩家随物体移动
    }

    public interface IGrabbable
    {
        // 抓取类型
        GrabType GrabType { get; }

        // 获取抓取点的位置（传入玩家位置以便计算最近点）
        Vector2 GetGrabPosition(Vector2 playerPosition);

        // 获取该对象的 Transform 引用（用于处理父子关系或相对移动）
        Transform GetTransform();

        // 验证当前是否可抓取
        bool CanGrab();
    }
}
