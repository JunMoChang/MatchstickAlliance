using System.Collections.Generic;
using AssetLoad;
using GamePlay.GameModel.Level;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.PlayerDataHandle;
using GamePlay.Scene;
using GamePlay.UI.Inventory.ScriptObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance { get; private set; }
        [SerializeField] private GameObject equipPickupPrefab;
        
        [System.Serializable]
        private struct CoinEntry
        {
            public int denomination;
            public GameObject prefab;
        }

        [SerializeField] private CoinEntry[] coinEntries;
        
        private int totalEnemies;
        private Stack<GameObject> coinStack;
        private float equipDropChance;
        private LevelDrapConfig dropConfig;
        /// <summary>
        /// 记录掉落的装备
        /// </summary>
        public List<(ItemScriptableObject, ItemRarityScriptObject.ItemRarity)> DroppedEquipments { get; private set; }
        void Awake()
        {
            Instance = this;
            SceneLoader.OnLevelLoaded += OnLevelLoaded;   
        }
        
        private void OnLevelLoaded()
        {
            ChapterData currentChapter = LevelContext.CurrentChapter;
            LevelData currentLevel = LevelContext.CurrentLevel;
            dropConfig = currentChapter.dorpConfig;

            totalEnemies = currentLevel.GetTotalEnemies();
            
            coinStack = DistributeGold(currentLevel.GetTotalEnemies());

            float chance = dropConfig.baseEquipDropChance + dropConfig.equipDropChancePerLevel * currentLevel.levelIndex;
            equipDropChance = Mathf.Clamp(chance, 0f, dropConfig.maxEquipDropChance);
        }

        private Stack<GameObject> DistributeGold(int allCoins)
        {
            List<GameObject> result = new ();
            foreach(CoinEntry coinEntry in coinEntries)
            {
                int count = allCoins / coinEntry.denomination;
                allCoins %=  coinEntry.denomination;
                
                for(int i = 0; i < count; i++)
                {
                    result.Add(coinEntry.prefab);
                }
            }
            return new Stack<GameObject>(result);
        }
        
        public void TryDrop(Vector2 position)
        {
            totalEnemies--;
            
            if (totalEnemies <= 0)
            {
                while (coinStack.Count > 0)
                {
                    SpawnGold(position, coinStack.Pop());
                }    
            }
            else
            {
                float drapRate = (float)coinStack.Count / totalEnemies;
                
                if(Random.value < drapRate && coinStack.Count > 0) SpawnGold(position, coinStack.Pop());
            }
            
            if (Random.value <= equipDropChance) SpawnEquipment(position);
        }

        private void SpawnGold(Vector2 position, GameObject coinPrefab)
        {
            Instantiate(coinPrefab, position, Quaternion.identity);
        }
        
        private void SpawnEquipment(Vector2 position)
        {
            if (GameDataManager.EquipmentPool == null)
            {
                Debug.LogError("EquipmentPool 尚未加载");
                return;
            }

            int star = GetStar();
            ItemRarityScriptObject.ItemRarity rarity = GetItemRarity();

            ItemScriptableObject equipment = GameDataManager.EquipmentPool.RollEquipment(star);
            if(!equipment) return;
            
            DroppedEquipments ??= new List<(ItemScriptableObject, ItemRarityScriptObject.ItemRarity)>(3);
            DroppedEquipments.Add((equipment, rarity));
            
            GameObject go = Instantiate(equipPickupPrefab, position, Quaternion.identity);
            go.GetComponent<SpriteRenderer>().sprite = equipment.GetItemSprite(rarity);
        }

        private int GetStar()
        {
            int currentChapter = LevelContext.CurrentChapter.chapter;
            
            float total = 0;
            foreach (LevelDrapConfig.StarWeight sWeight in dropConfig.stars)
            {
                if (currentChapter >= sWeight.minChapter && currentChapter <= sWeight.maxChapter)
                    total += sWeight.weight;
            }
            
            if (total <= 0) return -1;
            
            float chance = Random.Range(0f, total);
            
            float access = 0;
            foreach (LevelDrapConfig.StarWeight sWeight in dropConfig.stars)
            {
                if (currentChapter < sWeight.minChapter || currentChapter > sWeight.maxChapter) continue;

                access += sWeight.weight;
                if (chance <= access) return sWeight.star;
            }
            
            return dropConfig.stars[^1].star;
        }

        private ItemRarityScriptObject.ItemRarity GetItemRarity()
        {
            float total = 0;
            foreach (LevelDrapConfig.RarityWeight rWeight in dropConfig.rarityWeights) total += rWeight.weight;
            
            float chance = Random.Range(0f, total);
            float access = 0;
            foreach (LevelDrapConfig.RarityWeight rWeight in dropConfig.rarityWeights)
            {
                access += rWeight.weight;
                if (chance <= access) return rWeight.rarity;
            }
            
            return dropConfig.rarityWeights[^1].rarity;   
        }
        
        public void CommitEquipmentsToInventory()
        {
            if(DroppedEquipments == null) return;
            foreach ((ItemScriptableObject item, ItemRarityScriptObject.ItemRarity rarity) in DroppedEquipments)
            {
                PlayerDataManager.Instance.AddInventoryItem(item, rarity, 1);
            }
            DroppedEquipments.Clear();
            DroppedEquipments = null;
        }
        
        private void OnDestroy()
        {
            SceneLoader.OnLevelLoaded -= OnLevelLoaded;
        }
    }
}