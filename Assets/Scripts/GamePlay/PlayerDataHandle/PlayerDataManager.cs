using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;
using UnityEngine;

namespace GamePlay.PlayerDataHandle
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        private readonly SaveManager saveManager = new();
        public PlayerData PlayerData { get; private set; }

        public event Action OnCurrencyChanged;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            PlayerData = saveManager.LoadData();
            //PlayerData.ownedRoles.Add(RoleName.亚索, new RoleSaveData { roleName = RoleName.亚索, roleLevel = 1 });
        }

        void OnApplicationQuit()
        {
            saveManager.Save(PlayerData);
            Debug.Log("退出保存完成");
        }

  
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            PlayerData.gameProps.gold += amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
        }

        public void AddDiamond(int amount)
        {
            if (amount <= 0) return;
            PlayerData.gameProps.diamonds += amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || PlayerData.gameProps.gold < amount) return false;
            PlayerData.gameProps.gold -= amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
            return true;
        }

        public bool SpendDiamond(int amount)
        {
            if (amount <= 0 || PlayerData.gameProps.diamonds < amount) return false;
            PlayerData.gameProps.diamonds -= amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
            return true;
        }
        public void AddInventoryItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            if (itemSo.itemMaxSuperposition > 1)
            {
                foreach (PlayerData.ItemInstance instance in PlayerData.inventoryItems)
                {
                    if (instance.itemName == itemSo.itemName && instance.itemRarity == rarity)
                    {
                        instance.owenQuantities += quantity;
                        saveManager.Save(PlayerData);
                        return;
                    }
                }
            }
            
            PlayerData.inventoryItems.Add(new PlayerData.ItemInstance
            {
                itemName = itemSo.itemName,
                itemRarity = rarity,
                owenQuantities = quantity
            });
            saveManager.Save(PlayerData);
        }
        
        /// <summary>
        /// 设置指定角色的已装备物品
        /// </summary>
        public void SetEquippedItemsForRole(RoleName role, List<PlayerData.ItemInstance> items)
        {
            PlayerData.SetEquippedItemsForRole(role, items);
            saveManager.Save(PlayerData);
        }

        /// <summary>
        /// 获取指定角色的已装备物品
        /// </summary>
        public List<PlayerData.ItemInstance> GetEquippedItemsForRole(RoleName role)
        {
            return PlayerData.GetEquippedItemsForRole(role);
        }

        /// <summary>
        /// 清除指定角色的所有已装备物品
        /// </summary>
        public void ClearEquippedItemsForRole(RoleName role)
        {
            PlayerData.roleEquippedItems.Remove(role);
            saveManager.Save(PlayerData);
        }
        
        public bool IsFirstClear(int chapterIndex, int levelIndex)
        {
            int key = chapterIndex * 1000 + levelIndex;
            if (PlayerData.firstClearedLevelIds.Contains(key)) return false;
            PlayerData.firstClearedLevelIds.Add(key);
            return true;
        }

        public void Save()
        {
            saveManager.Save(PlayerData);
        }
    }
}
