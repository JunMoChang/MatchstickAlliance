using Fusion;
using UnityEngine;

namespace GamePlay.PvP
{
    public struct PvPInput : INetworkInput
    {
        /// <summary>移动方向 (归一化后的 Vector2) </summary>
        public Vector2 moveDirection;

        /// <summary> 是否按下攻击 </summary>
        public NetworkBool attack;

        /// <summary>触发的技能索引 (0=未触发) </summary>
        public byte skillIndex;

        /// <summary>技能是否本帧触发 (缘检测辅助) </summary>
        public NetworkBool skillTriggered;
    }
}
