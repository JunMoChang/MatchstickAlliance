using System;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.ScriptObjects;

namespace GamePlay.Role.RoleData
{
    /// <summary>
    /// 角色基础属性（升级成长）
    /// </summary>
    [Serializable]
    public struct BaseAttributes
    {
        public float health;
        public float defense;
        public float damage;
        public float critRate;

        public float GetProperty(RoleProperty property)
        {
            return property switch
            {
                RoleProperty.生命 => health,
                RoleProperty.防御 => defense,
                RoleProperty.攻击 => damage,
                RoleProperty.暴击 => critRate,
                _ => 0f
            };
        }

        public static TotalAttributes operator +(BaseAttributes b, EquipmentBonus e)
        {
            return new TotalAttributes
            {
                health = b.health + e.health,
                defense = b.defense + e.defense,
                damage = b.damage + e.damage,
                critRate = b.critRate + e.critRate
            };
        }
    }

    /// <summary>
    /// 装备加成属性
    /// </summary>
    [Serializable]
    public struct EquipmentBonus
    {
        public float health;
        public float defense;
        public float damage;
        public float critRate;

        public float GetProperty(RoleProperty property)
        {
            return property switch
            {
                RoleProperty.生命 => health,
                RoleProperty.防御 => defense,
                RoleProperty.攻击 => damage,
                RoleProperty.暴击 => critRate,
                _ => 0f
            };
        }

        public float GetProperty(ItemScriptableObject.ItemPropertyType type)
        {
            return type switch
            {
                ItemScriptableObject.ItemPropertyType.Health => health,
                ItemScriptableObject.ItemPropertyType.Defense => defense,
                ItemScriptableObject.ItemPropertyType.Damage => damage,
                ItemScriptableObject.ItemPropertyType.Critical => critRate,
                _ => 0f
            };
        }

        public static EquipmentBonus operator +(EquipmentBonus a, EquipmentBonus b)
        {
            return new EquipmentBonus
            {
                health = a.health + b.health,
                defense = a.defense + b.defense,
                damage = a.damage + b.damage,
                critRate = a.critRate + b.critRate
            };
        }
        
        public static EquipmentBonus operator -(EquipmentBonus a, EquipmentBonus b)
        {
            return new EquipmentBonus
            {
                health = a.health - b.health,
                defense = a.defense - b.defense,
                damage = a.damage - b.damage,
                critRate = a.critRate - b.critRate
            };
        }

        
        /// <summary>
        /// 强化曲线：t²，给定属性基础/最大值和当前强化等级，返回当前属性
        /// </summary>
        /// <returns>强化后数值</returns>
        public static float CalculatePropertyValue(float baseValue, float maxValue, int enhancementLevel, int maxEnhancementLevel)
        {
            if (maxValue <= baseValue) return baseValue;
            if (maxEnhancementLevel < 1) maxEnhancementLevel = 1;
            float t = (float)enhancementLevel / maxEnhancementLevel;
            float curve = t * t;
            return baseValue + (maxValue - baseValue) * curve;
        }

        /// <summary>
        /// 从装备属性数组中提取加成值
        /// </summary>
        /// <param name="properties">装备拥有的属性</param>
        /// <param name="enhancementLevel">当前强化次数</param>
        /// <param name="maxEnhancementLevel">最多强化次数</param>
        /// <param name="baseValueMult">不同稀有度的属性基础数值与 White稀有度的倍率（White为 1）</param>
        /// <param name="maxValueMult">不同稀有度的属性最大值数值与 White稀有度的倍率（White为 1）</param>
        /// <returns></returns>
        public static EquipmentBonus FromItemProperties(ItemScriptableObject.ItemProperty[] properties, int enhancementLevel = 0, int maxEnhancementLevel = 1, float baseValueMult = 1f, float maxValueMult = 1f)
        {
            EquipmentBonus bonus = default;

            foreach (ItemScriptableObject.ItemProperty prop in properties)
            {
                float scaledBase = prop.baseValue * baseValueMult;
                float scaledMax = prop.maxValue * maxValueMult;
                float value = CalculatePropertyValue(scaledBase, scaledMax, enhancementLevel, maxEnhancementLevel);
                
                switch (prop.itemPropertyType)
                {
                    case ItemScriptableObject.ItemPropertyType.Health:
                        bonus.health += value;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Defense:
                        bonus.defense += value;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Damage:
                        bonus.damage += value;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Critical:
                        bonus.critRate += value;
                        break;
                }
            }
            return bonus;
        }
    }

    /// <summary>
    /// 总属性 = 基础属性 + 装备加成
    /// </summary>
    [Serializable]
    public struct TotalAttributes
    {
        public float health;
        public float defense;
        public float damage;
        public float critRate;
    }
}
