namespace RPG2D.Pyhsics.Buoyancy
{
    /// <summary>
    /// 2D 浮力系统对外接口, 用于让其他模块统一控制浮力开关.
    /// </summary>
    public interface IBuoyancy2D
    {
        public bool IsBuoyancyEnabled { get; }

        /// <summary>
        /// 开启浮力, 使组件在物理更新中继续施加向上的力.
        /// </summary>
        public void EnableBuoyancy();

        /// <summary>
        /// 关闭浮力, 使组件停止继续施加向上的力.
        /// </summary>
        public void DisableBuoyancy();

        /// <summary>
        /// 根据传入状态统一设置浮力开关.
        /// </summary>
        public void SetBuoyancyEnabled(bool isEnabled);
    }
}
