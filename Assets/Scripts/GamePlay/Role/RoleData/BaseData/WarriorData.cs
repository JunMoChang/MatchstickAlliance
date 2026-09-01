using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "武士", menuName = "RoleData/武士")]
    public class WuShiData : RoleBaseData
    {
        public int maxCombos = 5;

        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;

        public MotionCommand[] motionCommands;
    }
}
