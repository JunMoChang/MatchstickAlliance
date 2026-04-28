using UnityEngine;

namespace GamePlay.Role.RoleData
{
    [CreateAssetMenu(fileName = "SunWuKongData", menuName = "RoleData/孙悟空")]
    public class SunWuKongData : RoleBaseData
    {
        public int maxCombos = 5;
        public readonly float[] skillCooldowns = {3,5,0,12};
        
        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;
        
        public MotionCommand[] motionCommands;
    }
}
