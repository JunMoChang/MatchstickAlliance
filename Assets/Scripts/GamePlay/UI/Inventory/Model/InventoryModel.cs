using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;

namespace GamePlay.UI.Inventory.Model
{
    public class InventoryModel
    {
        private readonly Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>> itemsDataDic;
        private readonly Dictionary<ItemScriptableObject.ItemType, int> itemsDataMaxCapacityDic;
        private readonly List<ItemDataModel> allItemsCache;

        public int DefaultCapacity { get; private set; } = 30;
        private bool isAllItemsCacheChange;

        public event Action<InventoryModel, ItemScriptableObject.ItemType> OnInventoryChanged;
        public event Action<ItemDataModel> OnItemEquipped;
        public event Action<ItemDataModel> OnItemUnequipped;
        

        public InventoryModel()
        {
            int typeCount = ItemScriptableObject.StorableItemType.Length;

            itemsDataDic = new Dictionary<ItemScriptableObject.ItemType, List<ItemDataModel>>(typeCount);
            itemsDataMaxCapacityDic = new Dictionary<ItemScriptableObject.ItemType, int>(typeCount);
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                itemsDataMaxCapacityDic.Add(type, DefaultCapacity);
                itemsDataDic.Add(type, new List<ItemDataModel>(DefaultCapacity));
            }

            allItemsCache = new List<ItemDataModel>(DefaultCapacity);
        }

        public ItemDataModel AddItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            if (itemSo == null || quantity <= 0) return null;

            if (itemSo.itemType == ItemScriptableObject.ItemType.Gold || itemSo.itemType == ItemScriptableObject.ItemType.Diamond) return null;

            List<ItemDataModel> dataList = itemsDataDic[itemSo.itemType];
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
                NotifyInventoryChanged(itemSo.itemType);
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
            OnItemEquipped?.Invoke(item);
            NotifyInventoryChanged(item.ItemSo.itemType);
        }

        public void UnequipItem(ItemDataModel item)
        {
            if (item == null || !item.IsEquipped) return;

            item.IsEquipped = false;
            item.EquippedByRole = null;

            List<ItemDataModel> dataList = itemsDataDic[item.ItemSo.itemType];
            foreach (ItemDataModel data in dataList)
            {
                if (data == item || data.IsEquipped) continue;
                if (!data.IsFull && data.ItemSo == item.ItemSo && data.ItemRarity == item.ItemRarity)
                {
                    data.AddQuantity(item.StorageItemQuantity);
                    RemoveItem(item);
                    item.ClearData();
                    OnItemUnequipped?.Invoke(data);
                    return;
                }
            }

            OnItemUnequipped?.Invoke(item);
            NotifyInventoryChanged(item.ItemSo.itemType);
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

            itemsDataDic[stackItem.ItemSo.itemType].Add(newItem);
            isAllItemsCacheChange = true;

            OnItemEquipped?.Invoke(newItem);
            NotifyInventoryChanged(stackItem.ItemSo.itemType);

            return newItem;
        }

        /// <summary>
        /// 获取指定类型的物品
        /// </summary>
        public IReadOnlyList<ItemDataModel> GetItemsByType(ItemScriptableObject.ItemType categoryType)
        {
            if (categoryType == ItemScriptableObject.ItemType.All) return GetAllItems();

            List<ItemDataModel> sorted = new List<ItemDataModel>(itemsDataDic[categoryType]);
            SortByRarityDesc(sorted);
            return sorted;
        }

        public IReadOnlyList<ItemDataModel> GetAllItems()
        {
            UpdateAllItems();
            
            return allItemsCache.AsReadOnly();
        }

        public List<ItemDataModel> GetEquippedItems()
        {
            List<ItemDataModel> result = new List<ItemDataModel>();
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                if (type == ItemScriptableObject.ItemType.Material || type == ItemScriptableObject.ItemType.Rune) continue;

                foreach (ItemDataModel item in itemsDataDic[type])
                {
                    if (item.IsEquipped) result.Add(item);
                }
            }
            SortByRarityDesc(result);
            return result;
        }

        /// <summary>
        /// 获取指定角色的已装备物品
        /// </summary>
        public List<ItemDataModel> GetEquippedItemsForRole(RoleName role)
        {
            List<ItemDataModel> result = new List<ItemDataModel>();
            foreach (ItemScriptableObject.ItemType type in ItemScriptableObject.StorableItemType)
            {
                if (type == ItemScriptableObject.ItemType.Material || type == ItemScriptableObject.ItemType.Rune) continue;

                foreach (ItemDataModel item in itemsDataDic[type])
                {
                    if (item.IsEquipped && item.EquippedByRole == role)
                        result.Add(item);
                }
            }
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

                foreach (ItemDataModel item in itemsDataDic[type])
                {
                    if (!item.IsEquipped)
                        result.Add(item);
                }
            }
            SortByRarityDesc(result);
            return result;
        }

        private void UpdateAllItems()
        {
            if (isAllItemsCacheChange)
            {
                isAllItemsCacheChange = false;
                allItemsCache.Clear();
                foreach (List<ItemDataModel> typeList in itemsDataDic.Values)
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
            if (itemsDataDic[item.ItemSo.itemType].Remove(item))
            {
                isAllItemsCacheChange = true;
                NotifyInventoryChanged(item.ItemSo.itemType);
            }
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

        private void NotifyInventoryChanged(ItemScriptableObject.ItemType categoryType)
        {
            OnInventoryChanged?.Invoke(this, categoryType);
        }
    }
}
