using GamePlay.Role.RoleData.BaseData;

namespace GamePlay.Role.RoleData
{
    public class RoleRuntimeData
    {
        public float maxHealth;
        public float currentHealth;
        public float damage;
        public float speed;

        public RuntimeSkillData[] skillRuntimeData;

        /// <summary>
        /// 技能运行时状态
        /// </summary>
        public class RuntimeSkillData
        {
            public float damage;
            public float cooldown;
            public float damageInterval;
            private float currentCooldownTimer;
            
            public bool IsReady => currentCooldownTimer <= 0f;

            public void Tick(float deltaTime)
            {
                if (currentCooldownTimer > 0f) currentCooldownTimer -= deltaTime;
            }

            public void TriggerCooldown()
            {
                currentCooldownTimer = cooldown;
            }
        }

        public void Init(RoleSaveData saveData, RoleBaseData template)
        {
            maxHealth = saveData.maxHealth;
            currentHealth = maxHealth;
            damage = saveData.damage;
            speed = saveData.speed;

            SkillSaveData[] skillsData = saveData.skillsData;
            RoleBaseData.SkillData[] skillConfigs = template.skillsBaseData;
            skillRuntimeData = new RuntimeSkillData[skillsData.Length];
            for (int i = 0; i < skillsData.Length; i++)
            {
                skillRuntimeData[i] = new RuntimeSkillData
                {
                    cooldown = skillConfigs[i].baseCooldown,
                    damage = skillsData[i].damage,
                    damageInterval = skillConfigs[i].damageInterval
                };
            }
        }
    }
}
