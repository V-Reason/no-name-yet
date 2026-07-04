using UnityEngine;

namespace RPG2D.Core.Interaction
{
    /// <summary>
    /// 可接收外部持续力的对象。
    /// 与 IActor.ApplyForce 不同，此接口面向无 Rigidbody2D 的 Verlet 物理对象（如锁链）。
    /// </summary>
    public interface IForceReceiver
    {
        /// <summary>施加持续力（等效于 ForceMode2D.Force）</summary>
        void ApplyForce(Vector2 force);
    }
}
