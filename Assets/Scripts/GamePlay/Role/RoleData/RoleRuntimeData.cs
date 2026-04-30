using UnityEngine;

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

        public void Init(RoleSaveData saveData)
        {
            maxHealth = saveData.health;
            currentHealth = maxHealth;
            damage = saveData.damage;
            speed = saveData.speed;

            int count = saveData.skillsCooldowns.Length;
            skillRuntimeData = new RuntimeSkillData[count];
            for (int i = 0; i < count; i++)
            {
                Debug.Log(i);
                skillRuntimeData[i] = new RuntimeSkillData
                {
                    cooldown = saveData.skillsCooldowns[i],
                    damage = saveData.skillsDamages[i],
                    damageInterval = saveData.damageIntervals[i]
                };
            }
        }
    }
}
