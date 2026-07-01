using UnityEngine;
using RPG2D.Core.Checker;

namespace RPG2D.Core.AnimatorWrapper
{
    public interface IAnimatorWrapper
    {
        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定动画层的执行位次
        /// </summary>
        /// <param name="checkData">来自Detector整理得的CheckData检测数据包，用于获取物理世界的必要参数</param>
        public void OnUpdate(CheckData checkData);

        // 对外接口
        public void SetBool(int hash, bool value);
        public void SetInteger(int hash, int value);
        public void SetFloat(int hash, float value);
        public void SetTrigger(int hash);
        public void ResetTrigger(int hash);

        public AnimatorStateInfo? GetCurrentAnimatorStateInfo();
    }
}
