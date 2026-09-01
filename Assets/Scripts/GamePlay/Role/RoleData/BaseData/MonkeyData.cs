using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "猴子", menuName = "RoleData/猴子")]
    public class MonkeyData : RoleBaseData
    {
        public int maxCombos = 5;

        public MotionCommand[] motionCommands;
        public HitBoxCommand[] hitBoxCommands;
    }
}
