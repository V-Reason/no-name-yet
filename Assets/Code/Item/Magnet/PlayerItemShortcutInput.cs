using UnityEngine;

namespace RPG2D.Item
{
    /// <summary>
    /// 玩家道具快捷键输入, 直接使用 Unity 官方 Input API 触发道具使用.
    /// </summary>
    [RequireComponent(typeof(PlayerItemHolder))]
    public class PlayerItemShortcutInput : MonoBehaviour
    {
        private PlayerItemHolder itemHolder;

        private void Awake()
        {
            itemHolder = GetComponent<PlayerItemHolder>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                itemHolder.TryUseMagnet();
            }
        }
    }
}
