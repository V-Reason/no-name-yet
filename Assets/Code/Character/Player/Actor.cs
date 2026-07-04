using System;
using UnityEngine;
using RPG2D.Core.Actor;
using RPG2D.Core.Checker;
using RPG2D.Pyhsics.Buoyancy;
using System.Collections.Generic;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 玩家对象行为执行器，以及数值存放（血量，体力等）
    /// 不用于逻辑判断，仅仅会执行相应动作，
    /// 是否应该执行交给上层处理
    /// </summary>
    [RequireComponent(typeof(ConstantBuoyancy2D))]
    public class Actor : BaseActor<PlayerData>
    {
        // 内部参数
        private PlayerData playerData => actorData;
        public ConstantBuoyancy2D buoyancy { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            buoyancy = GetComponent<ConstantBuoyancy2D>();
        }

        [ContextMenu("初始化物理参数")]
        protected override void SetupPhysics()
        {
            base.SetupPhysics();
        }
    }
}
