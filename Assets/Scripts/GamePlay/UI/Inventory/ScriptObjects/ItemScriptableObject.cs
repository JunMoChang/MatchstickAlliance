using System;
using UnityEngine;

namespace GamePlay.Inventory.ScriptObjects
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
        
        [Serializable]
        public struct RaritySprite
        {
            public ItemRarityScriptObject.ItemRarity rarity;
            public Sprite sprite;
        }
        /// <summary>
        /// 物品属性
        /// </summary>
        [Serializable]
        public struct ItemProperty
        {
            public ItemPropertyType itemPropertyType;
            public float baseValue;
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
            ItemType.Material,
            ItemType.Rune,
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
