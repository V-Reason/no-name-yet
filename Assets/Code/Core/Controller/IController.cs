namespace RPG2D.Core.Controller
{
    /// <summary>
    /// 控制信息处理器，
    /// 用于接收控制信息，标准化输入参数，
    /// 监测整理输入结果打包成InputData对外反映玩家控制
    /// </summary>
    public interface IController
    {
        // 对外参数
        public InputData inputData { get; }
        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定输入监测层的执行时机
        /// </summary>
        public void OnUpdate();
    }
}