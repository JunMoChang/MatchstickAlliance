using System;

namespace GamePlay.Role.RoleData
{
    public class RoleSaveData
    {
        public RoleName roleName;
        public int roleLevel;

        /// <summary> 角色基础属性 </summary>
        public BaseAttributes baseAttributes;

        /// <summary> 装备带来的属性加成 </summary>
        public EquipmentBonus equipmentBonus;

        /// <summary> 总属性 = 基础 + 装备 </summary>
        public TotalAttributes TotalAttributes => baseAttributes + equipmentBonus;

        public int power;
        public int maxExperience;
        public int currentExperience;
        public float speed;

        public SkillSaveData[] skillsData;

        public float GetProperty(RoleProperty property)
        {
            TotalAttributes total = TotalAttributes;
            return property switch
            {
                RoleProperty.战力 => power,
                RoleProperty.经验 => currentExperience,
                RoleProperty.生命 => total.health,
                RoleProperty.防御 => total.defense,
                RoleProperty.攻击 => total.damage,
                RoleProperty.暴击 => total.critRate,
                _ => 0f
            };
        }
    }

    [Serializable]
    public class SkillSaveData
    {
        public int level;
        public int damage;
    }
}