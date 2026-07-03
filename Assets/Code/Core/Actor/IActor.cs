using UnityEngine;
using RPG2D.Core.Controller;

namespace RPG2D.Core.Actor
{
    public interface IActor
    {
        Rigidbody2D rb { get; }

        // 应用速度
        void ApplyVelocity(Vector2 velocity);
        // 应用冲量
        void ApplyImpulse(Vector2 impulse);
        // 应用力
        void ApplyForce(Vector2 force);
        // 停止
        void Stop();

        // 控制对象立即执行的动作
        void Move();
    }
}
