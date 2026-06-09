using System;
using GamePlay.Inventory.ScriptObjects;

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
        /// 从装备属性数组中提取加成值
        /// </summary>
        public static EquipmentBonus FromItemProperties(ItemScriptableObject.ItemProperty[] properties)
        {
            EquipmentBonus bonus = default;
            if (properties == null) return bonus;

            foreach (ItemScriptableObject.ItemProperty prop in properties)
            {
                switch (prop.itemPropertyType)
                {
                    case ItemScriptableObject.ItemPropertyType.Health:
                        bonus.health += prop.baseValue;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Defense:
                        bonus.defense += prop.baseValue;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Damage:
                        bonus.damage += prop.baseValue;
                        break;
                    case ItemScriptableObject.ItemPropertyType.Critical:
                        bonus.critRate += prop.baseValue;
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
