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
            /// <summary> 释放期间是否允许自由移动 </summary>
            public bool canMoveWhileCasting;
            /// <summary> 自由移动允许的结束动画进度 </summary>
            public float freeMoveEndProgress = 1f;
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

        public void Initialize(RoleSaveData saveData, RoleBaseData template)
        {
            TotalAttributes attr = saveData.TotalAttributes;
            maxHealth = attr.health;
            currentHealth = maxHealth;
            damage = attr.damage;
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
                    damageInterval = skillConfigs[i].damageInterval,
                    canMoveWhileCasting = skillConfigs[i].canMoveWhileCasting,
                    freeMoveEndProgress = skillConfigs[i].freeMoveEndProgress
                };
            }
        }
    }
}
