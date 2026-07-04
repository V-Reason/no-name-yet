using UnityEngine;

namespace RPG2D.Core.Interaction
{
    /// <summary>
    /// 可接收外部持续力的对象，供力场统一向刚体或自定义物理对象施力。
    /// </summary>
    public interface IForceReceiver
    {
        /// <summary>施加持续力，具体物理映射由实现类自行处理。</summary>
        void ApplyForce(Vector2 force);
    }

    /// <summary>
    /// 可选的受力对象速度来源，用于力场在不依赖刚体类型的前提下做速度限制。
    /// </summary>
    public interface IForceReceiverVelocitySource
    {
        /// <summary>当前受力对象的世界速度。</summary>
        Vector2 Velocity { get; }
    }
}
