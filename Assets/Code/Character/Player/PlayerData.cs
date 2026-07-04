using UnityEngine;
using RPG2D.Core.Data;
using RPG2D.Core.Actor;

namespace RPG2D.Character.Player
{
    /// <summary>
    /// 玩家数据配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData")]
    public class PlayerData : ActorData
    {
        [Header("攀爬数值")]
        public float climbSpeed = 3f;
        public float grabRange = 0.5f;

        [Header("甩链力度")]
        public float swingPower = 5f;
    }
}
