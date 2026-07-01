using UnityEngine;

namespace RPG2D.Utilities
{
    public static class PersistentSingleton
    {
        /// <summary>
        /// 初始化持久化单例（替代继承的Awake逻辑）
        /// </summary>
        public static void Initialize<T>(ref T instance, T self) where T : MonoBehaviour
        {
            if (instance == null)
            {
                instance = self;
                Object.DontDestroyOnLoad(self.gameObject);
            }
            else
            {
                Object.Destroy(self);   // 只销毁当前组件，不伤及同物体其他组件
            }
        }
    }
}
// 使用例子
/*
using RPG2D.Utilities;
using UnityEngine;

public class InputHandler : MonoBehaviour   // 可以继承任何类，不占用继承位
{
    public static InputHandler Instance;

    private void Awake()
    {
        PersistentSingleton.Initialize(ref Instance, this);
    }

    public void Test()
    {
        Debug.Log("单例调用成功");
    }
}

InputHandler.Instance.Test();
*/