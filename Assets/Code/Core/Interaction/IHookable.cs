using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Core.Interaction
{
    public interface IHookable
    {
        // 钩子吸附的目标点
        Vector2 GetHookAttachPosition();
        // 当被钩住时的回调
        void OnHooked(ChainHook hook);
        // 当解开时的回调
        void OnUnhooked();
        // 是否当前允许被钩
        bool CanBeHooked();
    }
}
