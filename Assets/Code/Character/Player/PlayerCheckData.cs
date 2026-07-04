using RPG2D.Core.Checker;
using RPG2D.Core.Interaction;
using RPG2D.Item;
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

        public bool CanGrab { get; set; }
        public IGrabbable TargetGrabbable { get; set; }
    }
}
