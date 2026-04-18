namespace GamePlay.Role.RoleData
{
    public class RoleRuntimeData
    {
        /// <summary>
        /// 角色运行时数据
        /// </summary>
        public class RoleStatRuntime
        {
            public float maxHealth;
            public float currentHealth;
            public float damage;
            public float speed;
        }
        /// <summary>
        /// 技能运行时数据
        /// </summary>
        public class RuntimeSkillData
        {
            public float cooldown;
            public float damage;
            public float currentCooldownTimer;
    
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
        public RoleBaseData template;
        public RoleSaveData saveData;

        public void Init(RoleBaseData _template, RoleSaveData _saveData)
        {
            template = _template;
            saveData = _saveData;
            
        }
    }
}