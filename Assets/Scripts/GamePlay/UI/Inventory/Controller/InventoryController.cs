using System;
using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Inventory.View;
using GamePlay.PlayerDataHandle;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Inventory.Controller
{
    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance { get; private set; }
        
        [Header("InventoryView")]
        [SerializeField] private InventoryView inventoryView;

        [Header("分类按钮")]
        [SerializeField] private Button[] categoryButtons;
        
        [SerializeField] private EquipmentPool equipmentPool;
        
        private ItemSortType currentSortType;
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
            //RegisterSortButton();
        }

        private void InitFromSave(PlayerData playerData)
        {
            foreach (PlayerData.ItemInstance itemInstance in playerData.inventoryItems)
            {
                ItemScriptableObject so = equipmentPool.FindItemScriptableObject(itemInstance.itemName);
                ItemDataModel currentItemData = inventoryModel.AddItem(so, itemInstance.itemRarity, itemInstance.owenQuantities);
                
                if (playerData.equippedItems.Contains(itemInstance)) inventoryModel.EquipItem(currentItemData);
            }
        }
        
        private void RegisterCategoryButtons()
        {
            if(categoryButtons is { Length: 0 }) return;
            
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
            SyncEquippedToSave(item);
        }

        private void OnItemUnequipped(ItemDataModel item)
        {
            SyncEquippedToSave(item);
        }
        
        private void SyncEquippedToSave(ItemDataModel itemData)
        {
            List<PlayerData.ItemInstance> list = new ();
            foreach (ItemDataModel item in inventoryModel.GetEquippedItems())
            {
                list.Add(new PlayerData.ItemInstance
                {
                    itemName = item.ItemSo.itemName,
                    itemRarity = item.ItemRarity,
                    owenQuantities = itemData.StorageItemQuantity
                });
            }
            PlayerDataManager.Instance.SetEquippedItems(list);
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
