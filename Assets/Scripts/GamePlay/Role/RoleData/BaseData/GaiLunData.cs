using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "盖伦", menuName = "RoleData/盖伦")]
    public class GaiLunData : RoleBaseData
    {
        public int maxCombos = 5;

        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;

        public MotionCommand[] motionCommands;
    }
}
