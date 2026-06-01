using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "猴子", menuName = "RoleData/猴子")]
    public class MonkeyData : RoleBaseData
    {
        public int maxCombos = 5;

        public float skill2JumpForceX = 0.6f;
        public float skill2JumpForceY = 4.6f;

        public MotionCommand[] motionCommands;
    }
}
