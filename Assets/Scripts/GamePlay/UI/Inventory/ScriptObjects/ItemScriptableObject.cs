using System;
using GamePlay.Inventory.ScriptObjects;
using UnityEngine;

namespace GamePlay.UI.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item",  order = 0)]
    public class ItemScriptableObject : ScriptableObject
    {
        public ItemName itemName;
        public ItemType itemType;
        public int itemStar = 1;
        public ItemProperty[] itemProperties;
        public RaritySprite[] itemSprites;

        [Header("堆叠")]
        public int itemMaxSuperposition = 1;

        [Header("强化")]
        public int maxEnhancementLevel = 10;

        /// <summary>
        /// 是否有可强化的属性，有则返回 true
        /// </summary>
        public bool CanEnhance(float baseValueMult = 1f, float maxValueMult = 1f)
        {
            if (itemProperties == null || maxEnhancementLevel <= 0) return false;
            for (int i = 0; i < itemProperties.Length; i++)
            {
                if (itemProperties[i].maxValue * maxValueMult > itemProperties[i].baseValue * baseValueMult) return true;
            }
            return false;
        }
        
        [Serializable]
        public struct RaritySprite
        {
            public ItemRarityScriptObject.ItemRarity rarity;
            public Sprite sprite;
        }
        /// <summary>
        /// 物品属性：baseValue 为强化 0 级初始值，maxValue 为满强化时的值
        /// </summary>
        [Serializable]
        public struct ItemProperty
        {
            public ItemPropertyType itemPropertyType;
            public float baseValue;
            public float maxValue;
        }
        public enum ItemType
        {
            All,
            Weapon,
            Armor,
            Accessory,
            Material,
            Rune,
            Gold,
            Diamond,  
        }
        
        public static readonly ItemType[] StorableItemType =
        {
            ItemType.Weapon,
            ItemType.Armor,
            ItemType.Accessory,
        };
        
        public enum ItemPropertyType
        {
            Health,
            Defense,
            Damage,
            Critical
        }
        
        public enum ItemName
        {
            多兰剑,
            短剑,
            贪婪宝刀,
            冰牙
        }
        public Sprite GetItemSprite(ItemRarityScriptObject.ItemRarity rarity)
        {
            return itemSprites[(int)rarity].sprite;
        }
    }
}
