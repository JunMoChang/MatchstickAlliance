using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "ItemRarityTable", menuName = "Inventory/ItemRarityTable", order = 1)]
    public class ItemRarityScriptObject : ScriptableObject
    {
        public ItemRarityTable[] itemRarityTable;

        private Dictionary<ItemRarity, ItemRarityTable> lookup;
        
        private void OnValidate()
        {
            lookup = null;
        }

        private void EnsureLookup()
        {
            if (lookup != null) return;
            
            lookup = new Dictionary<ItemRarity, ItemRarityTable>(itemRarityTable?.Length ?? 0);
            
            if (itemRarityTable == null) return;
            
            foreach (ItemRarityTable entry in itemRarityTable)
            {
                lookup[entry.itemRarity] = entry;
            }
        }

        /// <summary>获取指定稀有度的基础值倍率</summary>
        public float GetBaseValueMult(ItemRarity rarity)
        {
            EnsureLookup();
            return lookup.TryGetValue(rarity, out ItemRarityTable entry) ? entry.baseValueMult : 1f;
        }

        /// <summary>获取指定稀有度的满强化值倍率</summary>
        public float GetMaxValueMult(ItemRarity rarity)
        {
            EnsureLookup();
            return lookup.TryGetValue(rarity, out ItemRarityTable entry) ? entry.maxValueMult : 1f;
        }

        [Serializable]
        public struct ItemRarityTable
        {
            public ItemRarity itemRarity;
            [Tooltip("基础值倍率，作用计算 ItemProperty.baseValue")]
            [Min(1)]
            public float baseValueMult;
            [Tooltip("满强化值倍率，作用计算 ItemProperty.maxValue")]
            [Min(1)]
            public float maxValueMult;
        }

        /// <summary>
        /// 稀有度从低到高
        /// </summary>
        public enum ItemRarity
        {
            White,
            Green,
            Blue,
            Purple,
            Orange,
        }
    }
}
