using System;
using GamePlay.Inventory.ScriptObjects;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelDropConfig", menuName = "Scriptable Objects/LevelDropConfig", order = 2)]
    public class LevelDrapConfig : ScriptableObject
    {
        [Header("装备掉落率")] 
        [Range(0, 1f)]
        public float baseEquipDropChance;
        [Range(0, 1f)]
        public float equipDropChancePerLevel;
        [Range(0, 1f)]
        public float maxEquipDropChance;
        
        [Header("装备星级")]
        public StarWeight[] stars;

        [Header("装备稀有度")]
        public RarityWeight[] rarityWeights;
        
        [Serializable]
        public struct StarWeight
        {
            public int star;
            public int weight;
            public int minChapter;
            public int maxChapter;
        }
        
        [Serializable]
        public struct RarityWeight
        {
            public ItemRarityScriptObject.ItemRarity rarity;
            public float weight;
        }
    }
}