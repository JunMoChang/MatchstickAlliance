using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "SunWuKongData", menuName = "RoleData/孙悟空")]
    public class GaiLunData : RoleBaseData
    {
        public int maxCombos = 5;

        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;

        public MotionCommand[] motionCommands;
    }
}
