using System;
using System.Collections.Generic;
using AssetLoad;
using GamePlay.GameModel.Level;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.ScriptObjects;
using UnityEngine;

namespace GamePlay.PlayerDataHandle
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        private readonly SaveManager saveManager = new();
        public PlayerData PlayerData { get; private set; }
        
        /// <summary> 所有已拥有角色战力之和 </summary>
        public int TotalPower { get;  private set; }
        
        public event Action OnCurrencyChanged;
        public event Action<int> OnPowerChanged;

        private InventoryModel inventoryModel;
        private bool isInventoryInitializing;
        
        void Awake()
        {
            if (Instance == null) Instance = this;
            else if(Instance != this)Destroy(gameObject);

            PlayerData = saveManager.LoadData();

            /*// 清除存档中可能残留的脏数据：没有装备物品的角色，equipmentBonus 应为零
            foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
            {
                if (!PlayerData.roleEquippedItems.ContainsKey(saveData.roleName))
                {
                    saveData.equipmentBonus = default;
                }
            }

            foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
            {
                UpdateRolePower(saveData);
            }
            CalculateOwnedTotalPower();*/
        }
        
        
        void OnEnable()
        {
            /*if(GameDataManager.RoleRegistry == null) return;
            foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
            {
                GameDataManager.RoleRegistry.GetRoleEntry(saveData.roleName).Value.template.FirstLoadSaveData(saveData);
            }
            foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
            {
                UpdateRolePower(saveData);
            }
            CalculateOwnedTotalPower();*/
        }
        
        void OnApplicationQuit()
        {
            saveManager.Save(PlayerData);
            Debug.Log("退出保存完成");
        }
        
        public void Save()
        {
            saveManager.Save(PlayerData);
        }
        
        public bool UnLockNewRole(RoleName roleName)
        {
            RoleRegistry.RoleEntry? roleEntry = GameDataManager.RoleRegistry.GetRoleEntry(roleName);
            if (roleEntry == null) return false;

            if (!SpendDiamond(roleEntry.Value.template.lockPrice))
            {
                Debug.Log("钻石不足");
                return false;
            }

            RoleBaseData newRole = roleEntry.Value.template;
            RoleSaveData newSaveData = new RoleSaveData { roleName = roleName, roleLevel = newRole.roleLevel };

            newRole.FirstLoadSaveData(newSaveData);
            bool success = PlayerData.ownedRoles.TryAdd(roleName, newSaveData);

            if (!success)
            {
                Debug.LogError($"添加角色:{roleName}失败");
            }
            else
            {
                CalculateOwnedTotalPower(UpdateRolePower(newSaveData));
            }
            return success;
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
            if (amount < 0 || PlayerData.gameProps.gold < amount) return false;
            PlayerData.gameProps.gold -= amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
            return true;
        }
        public bool SpendDiamond(int amount)
        {
            if (amount < 0 || PlayerData.gameProps.diamonds < amount) return false;
            PlayerData.gameProps.diamonds -= amount;
            OnCurrencyChanged?.Invoke();
            saveManager.Save(PlayerData);
            return true;
        }
        
        public void AddInventoryItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            if (itemSo.itemMaxSuperposition > 1)
            {
                foreach (PlayerData.ItemInstance instance in PlayerData.unequippedItems)
                {
                    if (instance.itemName == itemSo.itemName && instance.itemRarity == rarity)
                    {
                        instance.owenQuantities += quantity;
                        saveManager.Save(PlayerData);
                        return;
                    }
                }
            }
            
            PlayerData.unequippedItems.Add(new PlayerData.ItemInstance
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
        
        ///<summary> 判断是否首次通过关卡 </summary>>
        /// <param name="chapter">第几章(1-based)</param>
        /// <param name="level">第几关((1-based))</param>
        /// <returns>bool</returns>
        public bool IsLeveFirstPass(int chapter, int level)
        {
            int currentChapter = PlayerData.passedChapters == 0 ? 1 : PlayerData.passedChapters + 1;
            
            if (chapter < currentChapter) return false;
            
            if (chapter == currentChapter && PlayerData.passedLevels >= level) return false;

            if (chapter == currentChapter)
            {
                MarkLevelPassed(chapter, level);
            }
            
            return true;
        }
        
        /// <summary>
        /// 更新单个角色的战力缓存
        /// </summary>
        /// <param name="saveData">角色数据</param>
        /// <returns>增量</returns>
        private int UpdateRolePower(RoleSaveData saveData)
        {
            if (saveData == null) return 0;
            
            int lastPower = saveData.power;
            saveData.power = CalculateRolePower(saveData);
            
            return saveData.power - lastPower;
        }
        
        /// <summary>
        /// 计算单个角色的战力
        /// </summary>
        /// <param name="saveData">角色数据</param>
        /// <returns>角色战力</returns>
        private int CalculateRolePower(RoleSaveData saveData)
        {
            if (saveData == null) return 0;

            TotalAttributes attr = saveData.TotalAttributes;
            float power = attr.damage / 10f + attr.health / 100f + attr.defense / 5f + attr.critRate;

            return (int)power;
        }

        /// <summary>
        /// 计算已拥有角色总战力
        /// </summary>
        /// <param name="increment">增量（默认0）</param>
        private void CalculateOwnedTotalPower(int increment = 0)
        {
            if (increment != 0)
            {
                TotalPower += increment;
            }
            else
            {
                TotalPower = 0;
                foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
                {
                    TotalPower += saveData.power;
                }
            }
            
            OnPowerChanged?.Invoke(TotalPower);
        }
        
        /// <summary>
        /// 标记通关的关卡
        /// </summary>
        /// <param name="chapter">第几章</param>
        /// <param name="level">第几关</param>
        private void MarkLevelPassed(int chapter, int level)
        {
            PlayerData.passedLevels = level;
            if (LevelContext.CurrentChapter.levelData.Length == level) PlayerData.passedChapters = chapter;
        }
        
        /// <summary>
        /// 关卡是否解锁
        /// </summary>
        /// <param name="chapter">第几章((1-based))</param>
        /// <param name="level">第几关(1-based)()</param>
        public bool IsLevelUnlocked(int chapter, int level)
        {
            int currentChapter = PlayerData.passedChapters == 0 ? 1 : PlayerData.passedChapters + 1;
            
            if (chapter > currentChapter) return false;
            
            if (chapter < currentChapter) return true;

            if (level == 1) return true;
            return PlayerData.passedLevels >= level - 1;
        }
        
        /// <summary>
        /// 加载存档时从 PlayerData 填充 InventoryModel
        /// </summary>
        public void PopulateInventoryModel(InventoryModel model)
        {
            isInventoryInitializing = true;

            foreach (PlayerData.ItemInstance itemInstance in PlayerData.unequippedItems)
            {
                ItemScriptableObject so = GameDataManager.EquipmentPool?.FindEquipmentScriptableObject(itemInstance.itemName);
                ItemDataModel modelItem = model.AddItem(so, itemInstance.itemRarity, itemInstance.owenQuantities, itemInstance.level);
                if (modelItem != null)
                {
                    if (!string.IsNullOrEmpty(itemInstance.instanceId)) modelItem.InstanceId = itemInstance.instanceId;
                }
            }

            foreach (KeyValuePair<RoleName, List<PlayerData.ItemInstance>> kvp in PlayerData.roleEquippedItems)
            {
                foreach (PlayerData.ItemInstance equippedInstance in kvp.Value)
                {
                    ItemScriptableObject so = GameDataManager.EquipmentPool?.FindEquipmentScriptableObject(equippedInstance.itemName);
                    if (so == null) continue;

                    ItemDataModel newItem = model.AddItem(so, equippedInstance.itemRarity, equippedInstance.owenQuantities, equippedInstance.level);
                    if (newItem != null)
                    {
                        newItem.InstanceId = equippedInstance.instanceId;
                        model.EquipItem(newItem, kvp.Key);
                    }
                }
            }
            
            foreach (RoleSaveData saveData in PlayerData.ownedRoles.Values)
            {
                RefreshEquipmentBonusFromModel(model, saveData);
            }

            isInventoryInitializing = false;
        }

        /// <summary>
        /// 订阅 InventoryModel 事件，运行时修改同步到 PlayerData
        /// </summary>
        public void BindToModel(InventoryModel model)
        {
            inventoryModel = model;
            model.OnItemEquipped += OnSaveEquippedItemForRole;
            model.OnItemUnequipped += OnSaveUnequippedItemForRole;
        }

        public void UnbindFromModel(InventoryModel model)
        {
            model.OnItemEquipped -= OnSaveEquippedItemForRole;
            model.OnItemUnequipped -= OnSaveUnequippedItemForRole;
            inventoryModel = null;
        }

        /// <summary>
        /// 背包物品增加（掉落/奖励），同时更新 Model 和 PlayerData
        /// </summary>
        public ItemDataModel AddItemToModel(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            if (inventoryModel == null) return null;

            ItemDataModel result = inventoryModel.AddItem(itemSo, rarity, quantity);
            if (result != null)
            {
                SyncUnequippedItemsByType(itemSo.itemType);
                Save();
            }
            return result;
        }
        
        /// <summary>
        /// 装备强化后更新角色装备加成
        /// </summary>
        public void UpdateRoleEquippedBonus(RoleName role)
        {
            if (!PlayerData.ownedRoles.TryGetValue(role, out RoleSaveData saveData) || inventoryModel == null) return;

            RefreshEquipmentBonusFromModel(inventoryModel, saveData);

            CalculateOwnedTotalPower(UpdateRolePower(saveData));
            Save();
        }

        /// <summary>
        /// 根据 InventoryModel 中实际装备的物品重算 saveData.equipmentBonus
        /// 用于加载存档时清除可能残留的脏数据
        /// </summary>
        private void RefreshEquipmentBonusFromModel(InventoryModel model, RoleSaveData saveData)
        {
            saveData.equipmentBonus = default;
            IReadOnlyList<ItemDataModel> equippedItems = model.GetEquippedItemsForRole(saveData.roleName);
            foreach (ItemDataModel item in equippedItems)
            {
                saveData.equipmentBonus += item.CachedBonus;
            }
        }
        
        private void OnSaveEquippedItemForRole(ItemDataModel item)
        {
            if (isInventoryInitializing) return;

            if (item.EquippedByRole != null)
            {
                RoleName role = item.EquippedByRole.Value;

                PlayerData.unequippedItems.RemoveAll(i => i.instanceId == item.InstanceId);

                if (!PlayerData.roleEquippedItems.ContainsKey(role)) PlayerData.roleEquippedItems[role] = new List<PlayerData.ItemInstance>();

                PlayerData.roleEquippedItems[role].Add(CreateItemInstance(item));

                if (PlayerData.ownedRoles.TryGetValue(role, out RoleSaveData roleSaveData))
                {
                    ApplyEquipmentBonus(roleSaveData, item);
                    CalculateOwnedTotalPower(UpdateRolePower(roleSaveData));
                }
                
                Save();
            }
        }

        private void OnSaveUnequippedItemForRole(ItemDataModel item, RoleName previousRole)
        {
            if (isInventoryInitializing) return;

            if (PlayerData.roleEquippedItems.TryGetValue(previousRole, out List<PlayerData.ItemInstance> list))
            {
                list.RemoveAll(i => i.instanceId == item.InstanceId);
                if (list.Count == 0) PlayerData.roleEquippedItems.Remove(previousRole);
            }

            SyncUnequippedItemsByType(item.ItemSo.itemType);

            if (PlayerData.ownedRoles.TryGetValue(previousRole, out RoleSaveData roleSaveData))
            {
                RemoveEquipmentBonus(roleSaveData, item);
                CalculateOwnedTotalPower(UpdateRolePower(roleSaveData));
            }

            Save();
        }
        
        private void SyncUnequippedItemsByType(ItemScriptableObject.ItemType type)
        {
            PlayerData.unequippedItems.RemoveAll(i =>
            {
                ItemScriptableObject so = GameDataManager.EquipmentPool?.FindEquipmentScriptableObject(i.itemName);
                return so != null && so.itemType == type;
            });

            foreach (ItemDataModel item in inventoryModel.GetUnequippedItemsByType(type))
            {
                PlayerData.unequippedItems.Add(CreateItemInstance(item));
            }
        }
        
        private static void ApplyEquipmentBonus(RoleSaveData saveData, ItemDataModel item)
        {
            if (saveData == null || item?.ItemSo == null) return;
            saveData.equipmentBonus += item.CachedBonus;
        }

        private static void RemoveEquipmentBonus(RoleSaveData saveData, ItemDataModel item)
        {
            if (saveData == null || item?.ItemSo == null) return;
            saveData.equipmentBonus -= item.CachedBonus;
        }

        private static PlayerData.ItemInstance CreateItemInstance(ItemDataModel item)
        {
            return new PlayerData.ItemInstance
            {
                instanceId = item.InstanceId,
                itemName = item.ItemSo.itemName,
                itemRarity = item.ItemRarity,
                owenQuantities = item.StorageItemQuantity,
                level = item.EnhancementLevel
            };
        }
    }
}
