using UnityEngine;
using RPG2D.Core.Controller;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 控制信息处理器，
    /// 用于接收控制信息，标准化输入参数，
    /// 监测整理输入结果打包成InputData对外反映玩家控制
    /// </summary>
    public class Controller : BaseController<PlayerInputData>
    {
        // // 玩家控制器全局单例模式，用于读取键盘鼠标输入
        // public static Controller Instance;
        // private void Awake(){base.Awake(); PersistentSingleton.Initialize(ref Instance, this)};

        public override void OnUpdate()
        {
            updateMoveInput();
        }

        // 检测输入
        void updateMoveInput() =>
            inputData.Move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
