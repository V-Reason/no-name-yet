using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Core.Interaction
{
    public interface IHookable
    {
        Vector2 GetHookAttachPosition();
        void OnHooked(ChainHook hook);
        void OnUnhooked();
        bool CanBeHooked();
        // 获取该挂载点所属的链条（如果有），用于无缝切换
        Chain GetRelatedChain();
    }
}
