using System.Collections.Generic;
using RPG2D.Core.Checker;
using UnityEngine;

namespace RPG2D.Pyhsics.Attraction
{
    /// <summary>
    /// 圆形磁铁吸引力场, 组件启用时持续检测范围内目标并施加朝向磁铁圆心的吸引力.
    /// </summary>
    public class MagnetAttractor2D : MultiCircleChecker
    {
        private readonly HashSet<Rigidbody2D> appliedRigidbodies = new();

        [Header("磁铁吸引力")]
        [SerializeField, Min(0f)]
        private float maxAttractionForce = 20f;

        [SerializeField]
        private bool useDistanceFalloff = true;

        [SerializeField, Range(0f, 1f)]
        private float edgeForceMultiplier = 0f;

        [SerializeField, Min(0.001f)]
        private float minimumForceDistance = 0.05f;

        [SerializeField, Min(0f)]
        private float maxTargetSpeed = 0f;

        private void OnValidate()
        {
            maxAttractionForce = Mathf.Max(0f, maxAttractionForce);
            edgeForceMultiplier = Mathf.Clamp01(edgeForceMultiplier);
            minimumForceDistance = Mathf.Max(0.001f, minimumForceDistance);
            maxTargetSpeed = Mathf.Max(0f, maxTargetSpeed);
        }

        private void FixedUpdate()
        {
            ApplyAttractionToTargets();
        }

        /// <summary>
        /// 根据目标世界坐标计算该点受到的吸引力, 用于运行时施力和调试验证.
        /// </summary>
        public Vector2 GetForceAt(Vector2 targetPosition)
        {
            if (maxAttractionForce <= 0f || radius <= Mathf.Epsilon)
            {
                return Vector2.zero;
            }

            Vector2 attractionCenter = GetAttractionCenter();
            Vector2 toCenter = attractionCenter - targetPosition;
            float distance = toCenter.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return Vector2.zero;
            }

            Vector2 direction = toCenter / distance;
            float forceMultiplier = GetDistanceMultiplier(distance);
            return direction * maxAttractionForce * forceMultiplier;
        }

        /// <summary>
        /// 检测当前圆形范围内的目标刚体, 并对每个刚体最多施加一次吸引力.
        /// </summary>
        private void ApplyAttractionToTargets()
        {
            appliedRigidbodies.Clear();
            Check();

            for (int i = 0; i < hitCnt; i++)
            {
                Collider2D targetCollider = this[i];
                Rigidbody2D targetRb = targetCollider != null ? targetCollider.attachedRigidbody : null;
                if (!CanApplyAttraction(targetRb))
                {
                    continue;
                }

                Vector2 force = GetForceAt(targetRb.position);
                if (force.sqrMagnitude <= Mathf.Epsilon)
                {
                    continue;
                }

                targetRb.AddForce(force, ForceMode2D.Force);
                appliedRigidbodies.Add(targetRb);
            }
        }

        /// <summary>
        /// 判断刚体是否可以在当前物理帧接受吸引力, 避免无效目标和重复施力.
        /// </summary>
        private bool CanApplyAttraction(Rigidbody2D targetRb)
        {
            if (targetRb == null || targetRb.bodyType == RigidbodyType2D.Static || appliedRigidbodies.Contains(targetRb))
            {
                return false;
            }

            if (maxTargetSpeed <= 0f)
            {
                return true;
            }

            return targetRb.velocity.sqrMagnitude < maxTargetSpeed * maxTargetSpeed;
        }

        /// <summary>
        /// 计算与 MultiCircleChecker 检测逻辑一致的吸引圆心世界坐标.
        /// </summary>
        private Vector2 GetAttractionCenter()
        {
            float facing = autoFlipOffset ? Mathf.Sign(transform.localScale.x) : 1f;
            Vector2 localOffset = new Vector2(originOffset.x * facing, originOffset.y);
            return (Vector2)transform.position + localOffset;
        }

        /// <summary>
        /// 根据目标到圆心的距离计算吸力倍率, 靠近边缘时可按配置减弱.
        /// </summary>
        private float GetDistanceMultiplier(float distance)
        {
            if (!useDistanceFalloff)
            {
                return 1f;
            }

            float safeDistance = Mathf.Max(distance, minimumForceDistance);
            float distanceRate = 1f - Mathf.Clamp01(safeDistance / radius);
            return Mathf.Lerp(edgeForceMultiplier, 1f, distanceRate);
        }
    }
}
