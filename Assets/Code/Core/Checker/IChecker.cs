namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 所有Checker的接口
    /// </summary>
    public interface IChecker
    {
        // 存放Checker的检测结果
        bool IsConditionMet { get; }
        // 统一启动检测的接口
        void Check();
    }
}