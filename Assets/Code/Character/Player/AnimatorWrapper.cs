using RPG2D.Core.AnimatorWrapper;
using RPG2D.Core.Checker;

namespace RPG2D.Character.Player
{
    public class AnimatorWrapper : BaseAnimatorWrapper
    {
        // 对外参数
        //状态值
        public readonly int IsIdle = UnityEngine.Animator.StringToHash("isIdle");
        public readonly int IsMoving = UnityEngine.Animator.StringToHash("isMoving");

        public readonly int YVelocity = UnityEngine.Animator.StringToHash("yVelocity");
        public readonly int XVelocity = UnityEngine.Animator.StringToHash("xVelocity");

        public override void OnUpdate(CheckData checkData)
        {
            PlayerCheckData _checkData = checkData as PlayerCheckData;
            SetFloat(XVelocity, _checkData.Velocity.x);
            SetFloat(YVelocity, _checkData.Velocity.y);
        }
    }
}
