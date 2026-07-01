using UnityEngine;

namespace RPG2D.Core.Checker
{
    public class GroundChecker : RayChecker
    {
        // 内部参数
        [Header("地面检测参数设置")]
        [Tooltip("最大可站立倾斜角")]
        [SerializeField] private float maxSlopeAngle = 60.0f;

        // 门面模式，对外接口
        [HideInInspector]
        public Vector2 groundNormal
        {
            get => normal;
            protected set => normal = value;
        }
        [HideInInspector]
        public bool isGrounded
        {
            get => isTouched;
            protected set => isTouched = value;
        }

        public override void Check()
        {
            base.Check();
            // 计算地面倾斜角
            if (isTouched) // 验证判断
            {
                float slopeAngle = Vector2.Angle(normal, Vector2.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    isGrounded = false;
                    // groundNormal = Vector2.up;
                }
            }
            else // 重设未定值
            {
                groundNormal = Vector2.up;
            }
        }
    }
}