using System;
using GamePlay.Inventory.ScriptObjects;

namespace GamePlay.Inventory.Model
{
    public class ItemDataModel
    {
        public ItemScriptableObject ItemSo { get; private set; }
        public ItemRarityScriptObject.ItemRarity ItemRarity { get; private set; }
        public int StorageItemQuantity { get; private set; }
        public bool IsFull { get; private set; }
        public bool IsEquipped { get; set; }
        public int EnhancementLevel { get; set; }

        private int itemMaxSuperposition;

        public event Action<ItemDataModel> OnDataChanged;

        public int AddNewData(ItemScriptableObject targetItemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            ItemSo = targetItemSo;
            ItemRarity = rarity;
            itemMaxSuperposition = targetItemSo.itemMaxSuperposition;
            IsEquipped = false;
            EnhancementLevel = 0;

            int extraItems = AddQuantity(quantity);
            return extraItems;
        }

        public int AddQuantity(int quantity)
        {
            StorageItemQuantity += quantity;
            int extraItems = StorageItemQuantity - itemMaxSuperposition;

            if (extraItems > 0)
            {
                IsFull = true;
                StorageItemQuantity = itemMaxSuperposition;
            }
            else
            {
                extraItems = 0;
            }

            OnDataChanged?.Invoke(this);
            return extraItems;
        }

        public int DecreaseQuantity(int quantity = 1)
        {
            StorageItemQuantity = quantity < StorageItemQuantity ? StorageItemQuantity - quantity : 0;
            IsFull = false;

            OnDataChanged?.Invoke(this);
            return StorageItemQuantity;
        }

        public void ClearData()
        {
            IsFull = false;
            IsEquipped = false;
            EnhancementLevel = 0;
            itemMaxSuperposition = 0;
            StorageItemQuantity = 0;
            ItemSo = null;
            ItemRarity = ItemRarityScriptObject.ItemRarity.White;
            Dispose();
        }

        private void Dispose()
        {
            OnDataChanged = null;
        }

        public bool IsEmpty()
        {
            return StorageItemQuantity <= 0 || ItemSo == null;
        }
    }
}
