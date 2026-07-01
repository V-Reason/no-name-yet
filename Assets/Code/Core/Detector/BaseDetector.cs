#nullable enable
using System.Collections.Generic;
using RPG2D.Core.Checker;
using RPG2D.Core.Controller;
using UnityEngine;

namespace RPG2D.Core.Detector
{
    /// <summary>
    /// 环境检测器，
    /// 用于反映物理客观，拥有多个Checker，
    /// 整理Checkers的结果打包成CheckData对外反映物理客观
    /// </summary>
    /// 在这写需要添加的Checker，不过请只在子类加，保证基类的纯粹
    /// eg:
    /// [RequireComponent(typeof(GroundChecker))]
    /// [RequireComponent(typeof(MoveChecker))]
    public abstract class BaseDetector<TCheckData> : MonoBehaviour, IDetector where TCheckData : CheckData, new()
    {
        // 内部参数
        private List<IChecker> allCheckers = null!;
        private Dictionary<System.Type, IChecker> checkerMap = new();
        protected IController controller = null!;

        // 对外接口
        public TCheckData checkData { get; protected set; } = default!;
        CheckData IDetector.checkData => checkData;

        // 自注册
        protected virtual void Awake()
        {
            // 初始化
            checkData = new();

            // 自动注册所有挂载到对象上的Checker
            allCheckers = new(GetComponents<IChecker>());
            foreach (var c in allCheckers)
            {
                checkerMap[c.GetType()] = c;
            }
            // 注册其他组件
            controller = GetComponent<IController>();
        }

        /// <summary>
        /// 需要上层安排OnUpdate()至Update()里，
        /// 由上层决定检测层的执行位次
        /// </summary>
        public virtual void OnUpdate()
        {
            OnceDetect();
        }

        // 开始检测，多加了一层
        // Tick模式，需要放在update里
        public void OnceDetect()
        {
            // 遍历启动Checker
            foreach (var checker in allCheckers)
            {
                if (checker is BaseChecker bc && !bc.framCheck) continue;
                checker.Check();
            }
        }

        // 指定Checker进行单词查询
        public bool OnceCheck<TChecker>() where TChecker : IChecker
        {
            // Map获取Checker，调用Checker的通用接口取结果
            if (checkerMap.TryGetValue(typeof(TChecker), out var checker))
            {
                checker.Check();
                return checker.IsConditionMet;
            }
            return false;
        }

        // 泛型，T代表需要的Checker类型
        public bool GetCondition<TChecker>() where TChecker : IChecker
        {
            // Map获取Checker，调用Checker的通用接口取结果
            if (checkerMap.TryGetValue(typeof(TChecker), out var checker))
            {
                return checker.IsConditionMet;
            }
            return false;
        }

        // 获取单个Checker
        public TChecker? GetChecker<TChecker>() where TChecker : class, IChecker
        {
            // Map获取Checker，调用Checker的通用接口取结果
            if (checkerMap.TryGetValue(typeof(TChecker), out var checker))
            {
                return checker as TChecker;
            }

            return null;
        }
    }
}
