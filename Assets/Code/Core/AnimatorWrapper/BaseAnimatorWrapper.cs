#nullable enable
using UnityEngine;
using RPG2D.Core.Checker;

namespace RPG2D.Core.AnimatorWrapper
{
    public abstract class BaseAnimatorWrapper : MonoBehaviour, IAnimatorWrapper
    {
        // 内部参数
        protected Animator? anim;

        // 角色的Animator放在了子对象里！
        protected virtual void Awake()
            => anim = GetComponentInChildren<Animator>();

        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定动画层的执行位次
        /// </summary>
        /// <param name="checkData">来自Detector整理得的CheckData检测数据包，用于获取物理世界的必要参数</param>
        public abstract void OnUpdate(CheckData checkData);

        // 对外接口
        public void SetBool(int hash, bool value)
            => anim?.SetBool(hash, value);
        public void SetInteger(int hash, int value)
            => anim?.SetInteger(hash, value);
        public void SetFloat(int hash, float value)
            => anim?.SetFloat(hash, value);
        public void SetTrigger(int hash)
            => anim?.SetTrigger(hash);
        public void ResetTrigger(int hash)
            => anim?.ResetTrigger(hash);

        public AnimatorStateInfo? GetCurrentAnimatorStateInfo()
            => anim?.GetCurrentAnimatorStateInfo(0);
    }
}
