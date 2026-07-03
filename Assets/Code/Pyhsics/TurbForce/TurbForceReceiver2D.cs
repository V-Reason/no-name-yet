using System.Collections.Generic;
using UnityEngine;

namespace RPG2D.Pyhsics.TurbForce
{
    /// <summary>
    /// 2D 湍流力接收器, 汇总当前进入的湍流区域并施加到 Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TurbForceReceiver2D : MonoBehaviour
    {
        private readonly List<TurbForceZone2D> activeZones = new List<TurbForceZone2D>();
        private Rigidbody2D rb;

        /// <summary>
        /// 获取当前所有有效湍流区域叠加后的总力.
        /// </summary>
        public Vector2 CurrentTurbForce
        {
            get
            {
                Vector2 totalForce = Vector2.zero;
                Vector2 samplePosition = rb != null ? rb.position : (Vector2)transform.position;

                for (int i = activeZones.Count - 1; i >= 0; i--)
                {
                    TurbForceZone2D zone = activeZones[i];
                    if (zone == null || !zone.isActiveAndEnabled)
                    {
                        activeZones.RemoveAt(i);
                        continue;
                    }

                    totalForce += zone.GetForceAt(samplePosition);
                }

                return totalForce;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Vector2 totalForce = CurrentTurbForce;
            if (totalForce.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            rb.AddForce(totalForce, ForceMode2D.Force);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TurbForceZone2D zone = other.GetComponentInParent<TurbForceZone2D>();
            AddTurbForceZone(zone);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TurbForceZone2D zone = other.GetComponentInParent<TurbForceZone2D>();
            RemoveTurbForceZone(zone);
        }

        /// <summary>
        /// 注册进入的湍流区域, 已存在或无效区域会被忽略.
        /// </summary>
        public void AddTurbForceZone(TurbForceZone2D zone)
        {
            if (zone == null || activeZones.Contains(zone))
            {
                return;
            }

            activeZones.Add(zone);
        }

        /// <summary>
        /// 注销离开的湍流区域, 不存在时不会产生额外影响.
        /// </summary>
        public void RemoveTurbForceZone(TurbForceZone2D zone)
        {
            if (zone == null)
            {
                return;
            }

            activeZones.Remove(zone);
        }
    }
}
