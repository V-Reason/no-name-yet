using RPG2D.Core.AnimatorWrapper;
using RPG2D.Core.Checker;
using UnityEngine;

namespace RPG2D.Character.Player
{
    public class AnimatorWrapper : BaseAnimatorWrapper
    {
        // 动画状态
        public readonly int IsSwimming = Animator.StringToHash("IsSwimming");
        public readonly int IsClimbing = Animator.StringToHash("IsClimbing");
        public readonly int IsSwinging = Animator.StringToHash("IsSwinging");

        // 混合树参数
        public readonly int XVelocity = Animator.StringToHash("xVelocity");
        public readonly int YVelocity = Animator.StringToHash("yVelocity");

        public override void OnUpdate(CheckData checkData)
        {
            PlayerCheckData _checkData = checkData as PlayerCheckData;
            SetFloat(XVelocity, _checkData.Velocity.x);
            SetFloat(YVelocity, _checkData.Velocity.y);
        }
    }
}
