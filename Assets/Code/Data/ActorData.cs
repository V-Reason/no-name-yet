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
        public float moveSpeed;

    }
}
