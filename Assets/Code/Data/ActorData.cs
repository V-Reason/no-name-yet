using UnityEditor.EditorTools;
using UnityEngine;

namespace RPG2D.Core.Data
{
    [CreateAssetMenu(fileName = "ActorData", menuName = "Data/ActorData")]
    public class ActorData : ScriptableObject
    {
        // 不要在基类写枚举
        // public enum Skill
        // {

        // }

        [Header("移动")]
        public float moveSpeed = 5f;

        [Header("游泳数值")]
        [Tooltip("游动灵活性")]
        public float swimAcceleration = 5f;
        [Tooltip("转向力度系数")]
        public float turnExtraBoost = 1.5f;

        [Header("物理属性")]

        [Tooltip("物理材质")]
        public PhysicsMaterial2D material;

        [Tooltip("线性阻力")]
        public float linearDrag = 2f;
        [Tooltip("角阻力")]
        public float angularDrag = 10f;
        [Tooltip("质量")]
        public float mass = 1f;
        [Tooltip("重力缩放")]
        public float gravityScale = 0.0f;
        [Tooltip("插值模式")]
        public RigidbodyInterpolation2D interpolation = RigidbodyInterpolation2D.Interpolate;
        [Tooltip("碰撞检测")]
        public CollisionDetectionMode2D collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        [Tooltip("约束")]
        public RigidbodyConstraints2D constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
