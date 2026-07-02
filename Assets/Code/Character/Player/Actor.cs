using System;
using UnityEngine;
using RPG2D.Core.Actor;
using RPG2D.Core.Checker;
using System.Collections.Generic;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 玩家对象行为执行器，以及数值存放（血量，体力等）
    /// 不用于逻辑判断，仅仅会执行相应动作，
    /// 是否应该执行交给上层处理
    /// </summary>
    public class Actor : BaseActor<PlayerData>
    {
        // 内部参数
        private PlayerData playerData => actorData;

        protected override void Awake()
        {
            base.Awake();
        }

        // 跳跃
        public virtual void Jump(Vector2? offset = null)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度

            Vector2 finalImpulse = Vector2.up * playerData.jumpForce;
            if (offset.HasValue)
            {
                rb.velocity = Vector2.zero;
                finalImpulse += offset.Value;
            }
            ApplyImpulse(finalImpulse);
        }

    }
}
