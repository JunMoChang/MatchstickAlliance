using System;
using AssetLoad;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.ScriptObjects;

namespace GamePlay.UI.Inventory.Model
{
    public class ItemDataModel
    {
        public ItemScriptableObject ItemSo { get; private set; }
        public ItemRarityScriptObject.ItemRarity ItemRarity { get; private set; }
        public int StorageItemQuantity { get; private set; }
        public bool IsFull { get; private set; }
        public int EnhancementLevel { get; set; }
        /// <summary> 该物品被哪个角色装备（null 表示未装备） </summary>
        public RoleName? EquippedByRole { get; set; }
        /// <summary> 物品唯一标识，与存档 ItemInstance.instanceId 对应 </summary>
        public string InstanceId { get; set; }
        /// <summary> 当前强化等级下的装备属性加成 </summary>
        public EquipmentBonus CachedBonus { get; private set; }

        /// <summary> 稀有度基础值倍率 </summary>
        public float RarityBaseMult { get; private set; }
        /// <summary> 稀有度满强化值倍率 </summary>
        public float RarityMaxMult { get; private set; }

        private int itemMaxSuperposition;

        public event Action<ItemDataModel> OnDataChanged;

        public int AddNewData(ItemScriptableObject targetItemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity, int enhancementLevel = 0)
        {
            ItemSo = targetItemSo;
            ItemRarity = rarity;
            itemMaxSuperposition = targetItemSo.itemMaxSuperposition;
            EnhancementLevel = enhancementLevel;
            EquippedByRole = null;
            InstanceId = Guid.NewGuid().ToString("N");

            ItemRarityScriptObject rarityTable = GameDataManager.ItemRarityTable;
            RarityBaseMult = rarityTable != null ? rarityTable.GetBaseValueMult(rarity) : 1f;
            RarityMaxMult  = rarityTable != null ? rarityTable.GetMaxValueMult(rarity)  : 1f;

            CachedBonus = EquipmentBonus.FromItemProperties(targetItemSo.itemProperties, enhancementLevel, targetItemSo.maxEnhancementLevel, RarityBaseMult, RarityMaxMult);

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

        /// <summary>
        /// 减少堆叠数量
        /// </summary>
        /// <param name="quantity">减少数量（默认数值 1）</param>
        /// <returns>剩余堆叠数量</returns>
        public int DecreaseQuantity(int quantity = 1)
        {
            StorageItemQuantity = quantity < StorageItemQuantity ? StorageItemQuantity - quantity : 0;
            IsFull = false;

            OnDataChanged?.Invoke(this);
            return StorageItemQuantity;
        }

        /// <summary>
        /// 根据当前 EnhancementLevel 和稀有度倍率重新计算装备加成数据
        /// 用于从存档恢复强化等级后同步加成数据。
        /// </summary>
        public void RefreshCachedBonus()
        {
            if (ItemSo == null) return;
            CachedBonus = EquipmentBonus.FromItemProperties(ItemSo.itemProperties, EnhancementLevel, ItemSo.maxEnhancementLevel, RarityBaseMult, RarityMaxMult);
        }

        /// <summary>
        /// 强化装备，成功返回 true
        /// </summary>
        public bool Enhance()
        {
            if (ItemSo == null || EnhancementLevel >= ItemSo.maxEnhancementLevel || !ItemSo.CanEnhance(RarityBaseMult, RarityMaxMult)) return false;

            EnhancementLevel++;
            CachedBonus = EquipmentBonus.FromItemProperties(ItemSo.itemProperties, EnhancementLevel, ItemSo.maxEnhancementLevel, RarityBaseMult, RarityMaxMult);
            OnDataChanged?.Invoke(this);
            return true;
        }
        
        public void ClearData()
        {
            IsFull = false;
            EnhancementLevel = 0;
            EquippedByRole = null;
            InstanceId = null;
            itemMaxSuperposition = 0;
            StorageItemQuantity = 0;
            ItemSo = null;
            ItemRarity = ItemRarityScriptObject.ItemRarity.White;
            RarityBaseMult = 1f;
            RarityMaxMult = 1f;
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
