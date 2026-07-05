using System.Collections.Generic;
using UnityEngine;

namespace RPG2D.Core.Checker
{
    /// <summary>
    /// 多目标圆形重叠检测器
    /// </summary>
    public class MultiCircleChecker : CircleChecker
    {
        // 限制单次检测的最大目标数量，用于零GC设计
        [Header("多目标圆形重叠检测器")]
        [Tooltip("最大允许检测到的目标数量（Awake才会重新分配）")]
        [SerializeField] private int MAX_BUFFER_COUNT = 12;
        private Collider2D[] resultsBuffer;

        // 只读索引器设计
        // 调用：cld = Checker[idx]
        public Collider2D this[int index]
        {
            get
            {
                if (index < 0 || hitCnt <= index) return null;
                return resultsBuffer[index];
            }
        }

        protected virtual void Awake()
        {
            // 预先分配
            resultsBuffer = new Collider2D[MAX_BUFFER_COUNT];
        }

        public override void Check()
        {
            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1f;
            Vector2 localOffset = new Vector2(originOffset.x * facing, originOffset.y);
            Vector2 origin = (Vector2)transform.position + localOffset;

            // NonAlloc避免GC
            hitCnt = Physics2D.OverlapCircleNonAlloc(origin, radius, resultsBuffer, targetLayer);
            overlapped = hitCnt > 0;
            firstDetectedCollider = hitCnt > 0 ? resultsBuffer[0] : null;
        }
    }
}