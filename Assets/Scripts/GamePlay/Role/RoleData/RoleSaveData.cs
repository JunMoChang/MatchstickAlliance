namespace GamePlay.Role.RoleData
{
    public class RoleSaveData
    {
        /// <summary>
        /// 持久化属性
        /// </summary>
        public RoleName roleName;
        public int roleLevel;
        public float maxHealth;
        public float damage;
        public float defense;
        public float critRate;
        public int power;
        public int maxExperience;
        public int currentExperience;
        public float speed;
        
        public int[] skillsLevel;
        public float[] skillsDamages;
        public float[] skillsCooldowns;
        public float[] damageIntervals;
        
        public float GetProperty(RoleProperty property)
        {
            return property switch
            {
                RoleProperty.战力 => power,
                RoleProperty.经验 => currentExperience,
                RoleProperty.生命 => maxHealth,
                RoleProperty.防御 => defense,
                RoleProperty.攻击 => damage,
                RoleProperty.暴击 => critRate,
                _ => 0f
            };
        }
    }
}