using UnityEngine;

namespace GamePlay.Role.RoleData.BaseData
{
    [CreateAssetMenu(fileName = "武士", menuName = "RoleData/武士")]
    public class WarriorData : RoleBaseData
    {
        public int maxCombos = 5;

        public MotionCommand[] motionCommands;
        public HitBoxCommand[] hitBoxCommands;
    }
}
