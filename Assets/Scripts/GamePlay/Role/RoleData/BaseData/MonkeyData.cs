using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "MonkeyData", menuName = "RoleData/猴子")]
    public class SunWuKongData : RoleBaseData
    {
        public int maxCombos = 5;

        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;

        public MotionCommand[] motionCommands;
    }
}
