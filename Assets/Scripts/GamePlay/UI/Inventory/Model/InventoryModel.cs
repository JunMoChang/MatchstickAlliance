using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.ScriptObjects;

namespace GamePlay.UI.Inventory.Model
{
    public class InventoryModel
    {
        private readonly Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>> unequippedItemsDataDic;
        private readonly Dictionary<RoleName, List<ItemDataModel>> roleEquippedItems;
        
        public event Action<ItemDataModel> OnItemEquipped;
        public event Action<ItemDataModel, RoleName> OnItemUnequipped;
  
        public InventoryModel()
        {
            int typeCount = ItemScriptableObject.StorableItemType.Length;
            const int defaultCapacity = 30;

            unequippedItemsDataDic = new Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>>(typeCount);
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                unequippedItemsDataDic.Add(type, new List<ItemDataModel>(defaultCapacity));
            }

            roleEquippedItems = new Dictionary<RoleName, List<ItemDataModel>>();
        }
        
        public ItemDataModel AddItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity, int enhancementLevel = 0)
        {
            if (itemSo == null || quantity <= 0) return null;

            if (itemSo.itemType == ItemScriptableObject.ItemType.Gold || itemSo.itemType == ItemScriptableObject.ItemType.Diamond) return null;

            List<ItemDataModel> dataList = unequippedItemsDataDic[itemSo.itemType];
            ItemDataModel currentItemDataModel = null;

            if (itemSo.itemMaxSuperposition > 1)
            {
                foreach (ItemDataModel data in dataList)
                {
                    if (!data.IsFull && data.ItemSo == itemSo && data.ItemRarity == rarity && data.EnhancementLevel == enhancementLevel)
                    {
                        quantity = data.AddQuantity(quantity);
                        currentItemDataModel = data;
                    }
                }
            }

            while (quantity > 0)
            {
                ItemDataModel newItem = new ItemDataModel();
                currentItemDataModel = newItem;
                quantity = newItem.AddNewData(itemSo, rarity, quantity, enhancementLevel);

                dataList.Add(newItem);
            }

            return currentItemDataModel;
        }
        
        /// <summary>
        /// 判断添加的物品是否可以堆叠，可以则返回 true
        /// </summary>
        public bool CanStack(ItemDataModel data)
        {
            
            return true;
        }
        
        public void EquipItem(ItemDataModel item, RoleName role)
        {
            if (item == null || item.IsEmpty() || item.EquippedByRole.HasValue) return;

            item.EquippedByRole = role;

            unequippedItemsDataDic[item.ItemSo.itemType].Remove(item);

            if (!roleEquippedItems.TryGetValue(role, out List<ItemDataModel> list))
            {
                list = new List<ItemDataModel>();
                roleEquippedItems[role] = list;
            }
            list.Add(item);

            OnItemEquipped?.Invoke(item);
        }

        public void UnequipItem(ItemDataModel item)
        {
            if (item == null || item.EquippedByRole == null) return;

            RoleName previousRole = item.EquippedByRole.Value;
            
            if (roleEquippedItems.TryGetValue(previousRole, out List<ItemDataModel> list))
            {
                list.Remove(item);
                if (list.Count == 0) roleEquippedItems.Remove(previousRole);
            }

            // 尝试合并回背包中已有的同类同强化等级物品堆叠
            if (TryMergeIntoUnequippedStack(item, previousRole)) return;

            // 未能合并，放回背包
            item.EquippedByRole = null;

            unequippedItemsDataDic[item.ItemSo.itemType].Add(item);
            OnItemUnequipped?.Invoke(item, previousRole);
        }

        /// <summary>
        /// 从堆叠物品中拆分 1 个并标记为已装备，返回新创建的装备中物品
        /// </summary>
        /// <param name="stackItem">装备物品数据</param>
        /// <param name="role">装备该物品的角色</param>
        /// <returns>装备的物品数据</returns>
        public ItemDataModel SplitEquipItem(ItemDataModel stackItem, RoleName role)
        {
            if (stackItem == null || stackItem.StorageItemQuantity <= 1) return null;

            stackItem.DecreaseQuantity();

            ItemDataModel newItem = new ItemDataModel();
            newItem.AddNewData(stackItem.ItemSo, stackItem.ItemRarity, 1, stackItem.EnhancementLevel);
            newItem.EquippedByRole = role;

            if (!roleEquippedItems.TryGetValue(role, out List<ItemDataModel> eqList))
            {
                eqList = new List<ItemDataModel>();
                roleEquippedItems[role] = eqList;
            }
            eqList.Add(newItem);

            OnItemEquipped?.Invoke(newItem);

            return newItem;
        }
        
        public IReadOnlyList<ItemDataModel> GetUnequippedItemsByType(ItemScriptableObject.ItemType categoryType)
        {
            if (categoryType == ItemScriptableObject.ItemType.All) return GetUnequippedItems();

            List<ItemDataModel> sorted = new List<ItemDataModel>(unequippedItemsDataDic[categoryType]);
            SortByRarityDesc(sorted);
            return sorted.AsReadOnly();
        }

        public IReadOnlyList<ItemDataModel> GetUnequippedItems()
        {
            List<ItemDataModel> result = new List<ItemDataModel>();
            foreach (List<ItemDataModel> typeList in unequippedItemsDataDic.Values)
            {
                result.AddRange(typeList);
            }
            SortByRarityDesc(result);
            return result.AsReadOnly();
        }
        
        /// <summary>
        /// 获取指定角色的已装备的全部物品
        /// </summary>
        /// <param name="role">目标角色</param>
        /// <returns>物品信息集合</returns>
        public IReadOnlyList<ItemDataModel> GetEquippedItemsForRole(RoleName role)
        {
            if (!roleEquippedItems.TryGetValue(role, out List<ItemDataModel> list)) return Array.Empty<ItemDataModel>();

            List<ItemDataModel> copy = new List<ItemDataModel>(list);
            SortByRarityDesc(copy);
            return copy.AsReadOnly();
        }

        /// <summary>
        /// 尝试将卸下的物品合并到背包中已有的同类同强化等级未满堆叠。
        /// </summary>
        /// <param name="unequippedItem">卸下的装备</param>
        /// <param name="previousRole">上一个装备的角色</param>
        /// <returns>成功返回 true，否则返回 false</returns>
        private bool TryMergeIntoUnequippedStack(ItemDataModel unequippedItem, RoleName previousRole)
        {
            List<ItemDataModel> unequippedItems = unequippedItemsDataDic[unequippedItem.ItemSo.itemType];
            foreach (ItemDataModel existing in unequippedItems)
            {
                if (existing.IsFull) continue;
                if (existing.ItemSo != unequippedItem.ItemSo) continue;
                if (existing.ItemRarity != unequippedItem.ItemRarity) continue;
                if (existing.EnhancementLevel != unequippedItem.EnhancementLevel) continue;

                existing.AddQuantity(unequippedItem.StorageItemQuantity);

                OnItemUnequipped?.Invoke(unequippedItem, previousRole);
                unequippedItem.ClearData();

                return true;
            }

            return false;
        }

        private void SortByRarityDesc(List<ItemDataModel> list)
        {
            list.Sort((a, b) => b.ItemRarity.CompareTo(a.ItemRarity));
        }
    }
}
