using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;

namespace GamePlay.UI.Inventory.Model
{
    public class InventoryModel
    {
        private readonly Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>> unequippedItemsDataDic;
        private readonly Dictionary<RoleName, List<ItemDataModel>> equippedItems;
        private readonly Dictionary<ItemScriptableObject.ItemType, int> itemsDataMaxCapacityDic;
        private readonly List<ItemDataModel> allItemsCache;
        
        public int DefaultCapacity { get; private set; } = 30;
        private bool isAllItemsCacheChange;
        
        public event Action<ItemDataModel> OnItemEquipped;
        public event Action<ItemDataModel, RoleName> OnItemUnequipped;
  
        public InventoryModel()
        {
            int typeCount = ItemScriptableObject.StorableItemType.Length;

            unequippedItemsDataDic = new Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>>(typeCount);
            itemsDataMaxCapacityDic = new Dictionary<ItemScriptableObject.ItemType, int>(typeCount);
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                itemsDataMaxCapacityDic.Add(type, DefaultCapacity);
                unequippedItemsDataDic.Add(type, new List<ItemDataModel>(DefaultCapacity));
            }

            allItemsCache = new List<ItemDataModel>(DefaultCapacity);
            equippedItems = new Dictionary<RoleName, List<ItemDataModel>>();
        }
        
        public ItemDataModel AddItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            if (itemSo == null || quantity <= 0) return null;

            if (itemSo.itemType == ItemScriptableObject.ItemType.Gold || itemSo.itemType == ItemScriptableObject.ItemType.Diamond) return null;

            List<ItemDataModel> dataList = unequippedItemsDataDic[itemSo.itemType];
            ItemDataModel currentItemDataModel = null;
            
            if (itemSo.itemMaxSuperposition > 1)
            {
                foreach (ItemDataModel data in dataList)
                {
                    if (!data.IsFull && data.ItemSo == itemSo && data.ItemRarity == rarity)
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
                quantity = newItem.AddNewData(itemSo, rarity, quantity);

                dataList.Add(newItem);
                isAllItemsCacheChange = true;
            }
            
            return currentItemDataModel;
        }

        public bool UseItem(ItemDataModel item)
        {
            if (item == null || item.IsEmpty()) return false;

            if (item.ItemSo.itemMaxSuperposition > 1)
            {
                if (item.DecreaseQuantity() <= 0)
                {
                    RemoveItem(item);
                    item.ClearData();
                }
                return true;
            }

            return false;
        }

        public void EquipItem(ItemDataModel item, RoleName role)
        {
            if (item == null || item.IsEmpty() || item.IsEquipped) return;

            item.IsEquipped = true;
            item.EquippedByRole = role;

            unequippedItemsDataDic[item.ItemSo.itemType].Remove(item);
            isAllItemsCacheChange = true;

            if (!equippedItems.TryGetValue(role, out List<ItemDataModel> list))
            {
                list = new List<ItemDataModel>();
                equippedItems[role] = list;
            }
            list.Add(item);

            OnItemEquipped?.Invoke(item);
        }

        public void UnequipItem(ItemDataModel item)
        {
            if (item == null || !item.IsEquipped || item.EquippedByRole == null) return;

            RoleName previousRole = item.EquippedByRole.Value;
            
            if (equippedItems.TryGetValue(previousRole, out List<ItemDataModel> list))
            {
                list.Remove(item);
                if (list.Count == 0) equippedItems.Remove(previousRole);
            }

            // 尝试合并回背包中已有的同物品堆叠
            List<ItemDataModel> dataList = unequippedItemsDataDic[item.ItemSo.itemType];
            foreach (ItemDataModel itemData in dataList)
            {
                if (itemData == item || itemData.IsEquipped) continue;
                if (!itemData.IsFull && itemData.ItemSo == item.ItemSo && itemData.ItemRarity == item.ItemRarity)
                {
                    itemData.AddQuantity(item.StorageItemQuantity);
                    
                    item.IsEquipped = false;
                    item.EquippedByRole = null;
                    OnItemUnequipped?.Invoke(item, previousRole);

                    item.ClearData();
                    return;
                }
            }

            // 未能合并，放回背包
            item.IsEquipped = false;
            item.EquippedByRole = null;

            dataList.Add(item);
            isAllItemsCacheChange = true;
            OnItemUnequipped?.Invoke(item, previousRole);
        }

        /// <summary>
        /// 从堆叠物品中拆分 1 个并标记为已装备，返回新创建的装备中物品
        /// </summary>
        public ItemDataModel SplitEquipItem(ItemDataModel stackItem, RoleName role)
        {
            if (stackItem == null || stackItem.StorageItemQuantity <= 1) return null;

            stackItem.DecreaseQuantity(1);

            ItemDataModel newItem = new ItemDataModel();
            newItem.AddNewData(stackItem.ItemSo, stackItem.ItemRarity, 1);
            newItem.IsEquipped = true;
            newItem.EquippedByRole = role;

            if (!equippedItems.TryGetValue(role, out List<ItemDataModel> eqList))
            {
                eqList = new List<ItemDataModel>();
                equippedItems[role] = eqList;
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
            return sorted;
        }

        public IReadOnlyList<ItemDataModel> GetUnequippedItems()
        {
            UpdateUnequippedItemsCache();
            
            return allItemsCache.AsReadOnly();
        }

        public List<ItemDataModel> GetEquippedItems()
        {
            List<ItemDataModel> result = new List<ItemDataModel>();
            foreach (List<ItemDataModel> list in equippedItems.Values)
                result.AddRange(list);

            SortByRarityDesc(result);
            return result;
        }

        /// <summary>
        /// 获取指定角色的已装备物品
        /// </summary>
        public List<ItemDataModel> GetEquippedItemsForRole(RoleName role)
        {
            if (!equippedItems.TryGetValue(role, out List<ItemDataModel> list))
                return new List<ItemDataModel>();

            List<ItemDataModel> result = new List<ItemDataModel>(list);
            SortByRarityDesc(result);
            return result;
        }

        public List<ItemDataModel> GetUnequippedEquipment()
        {
            List<ItemDataModel> result = new List<ItemDataModel>();
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                if (type == ItemScriptableObject.ItemType.Material ||
                    type == ItemScriptableObject.ItemType.Rune)
                    continue;

                result.AddRange(unequippedItemsDataDic[type]);
            }
            SortByRarityDesc(result);
            return result;
        }

        private void UpdateUnequippedItemsCache()
        {
            if (isAllItemsCacheChange)
            {
                isAllItemsCacheChange = false;
                allItemsCache.Clear();
                foreach (List<ItemDataModel> typeList in unequippedItemsDataDic.Values)
                {
                    allItemsCache.AddRange(typeList);
                }

                SortByRarityDesc(allItemsCache);

                if (allItemsCache.Count > DefaultCapacity)
                    DefaultCapacity = allItemsCache.Count;
            }
        }

        private void RemoveItem(ItemDataModel item)
        {
            if (unequippedItemsDataDic[item.ItemSo.itemType].Remove(item))
                isAllItemsCacheChange = true;
        }
        
        public int GetItemCapacity(ItemScriptableObject.ItemType categoryType)
        {
            if(categoryType == ItemScriptableObject.ItemType.All) return DefaultCapacity;
            
            return itemsDataMaxCapacityDic.GetValueOrDefault(categoryType, DefaultCapacity);
        }

        private void SortByRarityDesc(List<ItemDataModel> list)
        {
            list.Sort((a, b) => b.ItemRarity.CompareTo(a.ItemRarity));
        }

    }
}
