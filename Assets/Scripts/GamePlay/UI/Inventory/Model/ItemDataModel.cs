using System;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;

namespace GamePlay.UI.Inventory.Model
{
    public class ItemDataModel
    {
        public ItemScriptableObject ItemSo { get; private set; }
        public ItemRarityScriptObject.ItemRarity ItemRarity { get; private set; }
        public int StorageItemQuantity { get; private set; }
        public bool IsFull { get; private set; }
        public bool IsEquipped { get; set; }
        public int EnhancementLevel { get; set; }
        /// <summary>
        /// 该物品被哪个角色装备（未装备时为 null）
        /// </summary>
        public RoleName? EquippedByRole { get; set; }
        /// <summary>
        /// 物品唯一标识，与存档 ItemInstance.instanceId 对应（加载时可覆盖）
        /// </summary>
        public string InstanceId { get; set; }

        private int itemMaxSuperposition;

        public event Action<ItemDataModel> OnDataChanged;

        public int AddNewData(ItemScriptableObject targetItemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            ItemSo = targetItemSo;
            ItemRarity = rarity;
            itemMaxSuperposition = targetItemSo.itemMaxSuperposition;
            IsEquipped = false;
            EnhancementLevel = 0;
            EquippedByRole = null;
            InstanceId = Guid.NewGuid().ToString("N");

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
            EquippedByRole = null;
            InstanceId = null;
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
