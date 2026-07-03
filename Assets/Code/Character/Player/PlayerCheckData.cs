using RPG2D.Core.Checker;
using UnityEngine;

/// <summary>
/// 用于封装所有Checker的参数，
/// 打包给别的组件作为数据
/// </summary>
namespace RPG2D.Character.Player
{
    public class PlayerCheckData : CheckData
    {
        public bool IsMoving { get; set; }
        public Vector2 Velocity { get; set; }

        public bool CanGrabChain { get; set; }
        public Chain TargetChain { get; set; }
    }
}
