using RPG2D.Item;
using UnityEngine;

namespace RPG2D.Core.Interaction
{
    public enum HookPointType { Head, Tail }

    public interface IHookable
    {
        Vector2 GetHookAttachPosition();
        void OnHooked(ChainHook hook);
        void OnUnhooked();
        bool CanBeHooked();
        Chain GetRelatedChain();
        HookPointType GetHookPointType();
    }
}
