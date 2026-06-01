using System;
using System.Collections.Generic;
using AssetLoad;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.View.ContainerView;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.Controller
{
    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance { get; private set; }

        [Header("InventoryView")]
        [SerializeField] private InventoryView inventoryView;

        [Header("分类按钮")]
        [SerializeField] private Button[] categoryButtons;

        private bool ascending;

        private InventoryModel inventoryModel;

        private Image lastBtnImage;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            inventoryView.Hide();
        }

        public void Initialize(InventoryModel inventoryMod)
        {
            Debug.Log("初始库存控制器");

            inventoryModel = inventoryMod;
            inventoryModel.OnItemEquipped += OnItemEquipped;
            inventoryModel.OnItemUnequipped += OnItemUnequipped;

            InitFromSave(PlayerDataManager.Instance.PlayerData);

            inventoryView.BindInventoryModel(inventoryModel);

            RegisterCategoryButtons();
        }

        private void InitFromSave(PlayerData playerData)
        {
            // 第一步：加载所有背包物品，建立 instanceId → ItemDataModel 映射
            Dictionary<string, ItemDataModel> idToModel = new Dictionary<string, ItemDataModel>();
            foreach (PlayerData.ItemInstance itemInstance in playerData.inventoryItems)
            {
                ItemScriptableObject so = GameDataManager.EquipmentPool?.FindItemScriptableObject(itemInstance.itemName);
                ItemDataModel currentItemData = inventoryModel.AddItem(so, itemInstance.itemRarity, itemInstance.owenQuantities);
                if (currentItemData != null && !string.IsNullOrEmpty(itemInstance.instanceId))
                {
                    // 使 Model 的 InstanceId 与存档 ItemInstance 的 instanceId 对齐
                    currentItemData.InstanceId = itemInstance.instanceId;
                    idToModel[itemInstance.instanceId] = currentItemData;
                }
            }

            // 第二步：根据 roleEquippedItems 标记各角色的已装备物品（GUID 精确匹配）
            foreach (KeyValuePair<RoleName, List<PlayerData.ItemInstance>> kvp in playerData.roleEquippedItems)
            {
                RoleName role = kvp.Key;
                foreach (PlayerData.ItemInstance equippedInstance in kvp.Value)
                {
                    if (!string.IsNullOrEmpty(equippedInstance.instanceId) &&
                        idToModel.TryGetValue(equippedInstance.instanceId, out ItemDataModel model))
                    {
                        inventoryModel.EquipItem(model, role);
                    }
                }
            }
        }

        private void RegisterCategoryButtons()
        {
            if (categoryButtons is { Length: 0 }) return;

            Array categories = Enum.GetValues(typeof(ItemScriptableObject.ItemType));

            lastBtnImage = categoryButtons[0].image;
            for (int i = 0; i < categoryButtons.Length && i < categories.Length; i++)
            {
                int index = i;
                var type = (ItemScriptableObject.ItemType)categories.GetValue(i);
                categoryButtons[i].onClick.AddListener(() => OnCategoryButtonClicked(categoryButtons[index], type));
            }
        }

        public ItemDataModel AddItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            return inventoryModel.AddItem(itemSo, rarity, quantity);
        }

        private void OnItemEquipped(ItemDataModel item)
        {
            SyncEquippedToSave();
        }

        private void OnItemUnequipped(ItemDataModel item)
        {
            SyncEquippedToSave();
        }

        /// <summary>
        /// 从 Model 重建 PlayerData.roleEquippedItems 字典并保存
        /// </summary>
        private void SyncEquippedToSave()
        {
            PlayerData playerData = PlayerDataManager.Instance.PlayerData;
            playerData.roleEquippedItems.Clear();

            foreach (ItemDataModel item in inventoryModel.GetEquippedItems())
            {
                if (item.EquippedByRole == null) continue;
                RoleName role = item.EquippedByRole.Value;

                if (!playerData.roleEquippedItems.ContainsKey(role))
                    playerData.roleEquippedItems[role] = new List<PlayerData.ItemInstance>();

                playerData.roleEquippedItems[role].Add(new PlayerData.ItemInstance
                {
                    instanceId = item.InstanceId,
                    itemName = item.ItemSo.itemName,
                    itemRarity = item.ItemRarity,
                    owenQuantities = item.StorageItemQuantity,
                    level = item.EnhancementLevel
                });
            }
            PlayerDataManager.Instance.Save();
        }

        private void OnCategoryButtonClicked(Button categoryBtn, ItemScriptableObject.ItemType category)
        {
            lastBtnImage.color = Color.black;
            categoryBtn.image.color = Color.red;
            lastBtnImage = categoryBtn.image;

            inventoryView.SwitchCategory(category);
            inventoryView.ClearItemInfo();
        }

        public void ShowBackpack()
        {
            inventoryView?.Show();
        }

        public void HideBackpack()
        {
            inventoryView?.Hide();
        }

        public InventoryModel GetInventoryModel()
        {
            return inventoryModel;
        }

        private void OnDestroy()
        {
            inventoryModel.OnItemEquipped -= OnItemEquipped;
            inventoryModel.OnItemUnequipped -= OnItemUnequipped;
        }
    }
}
