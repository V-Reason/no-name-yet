using RPG2D.Core.Controller;
using UnityEngine;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 输入参数数据包，
    /// 封装所有的输入树，打包给其它组件作参数
    /// </summary>
    public class PlayerInputData : InputData
    {
        public bool Jump;
    }
}
