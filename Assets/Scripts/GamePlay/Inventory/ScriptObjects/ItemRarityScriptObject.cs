using System;
using UnityEngine;

namespace GamePlay.Inventory.ScriptObjects
{
    [CreateAssetMenu(fileName = "ItemRarityTable", menuName = "Inventory/ItemRarityTable", order = 1)]
    public class ItemRarityScriptObject : ScriptableObject
    {
        public ItemRarityTable[] itemRarityTable;
        
        [Serializable]
        public struct ItemRarityTable
        {
            public ItemRarity itemRarity;
            public ItemProperty[] properties;
        }
        
        [Serializable]
        public struct ItemProperty
        {
            public ItemScriptableObject.ItemPropertyType itemPropertyType;
            public float rarityMult;
            public float levelMaxMult;
        }
        
        /// <summary>
        /// 稀有度从底到高
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