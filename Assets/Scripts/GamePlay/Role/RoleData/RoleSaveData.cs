namespace GamePlay.Role.RoleData
{
    public class RoleSaveData
    {
        /// <summary>
        /// 持久化属性
        /// </summary>
        public RoleName roleName;
        public int roleLevel;
        public float health;
        public float damage;
        public float speed;
        public int[] skillsLevel;
        public float[] skillsDamages;
        public float[] skillsCooldowns;
        public float[] damageIntervals;
    }
}