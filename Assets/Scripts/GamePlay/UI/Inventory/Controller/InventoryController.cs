using System;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.PlayerDataHandle;
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
            Debug.Log("InventoryController::Initialize");
            inventoryModel = inventoryMod;
            
            PlayerDataManager.Instance.PopulateInventoryModel(inventoryModel);
            PlayerDataManager.Instance.BindToModel(inventoryModel);

            inventoryView.BindInventoryModel(inventoryModel);
            RegisterCategoryButtons();
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

        /// <summary>
        /// 主界面获取物品时调用，更新 model和 PlayerData
        /// </summary>
        public ItemDataModel AddItem(ItemScriptableObject itemSo, ItemRarityScriptObject.ItemRarity rarity, int quantity)
        {
            return PlayerDataManager.Instance.AddItemToModel(itemSo, rarity, quantity);
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
            if (inventoryModel != null)
                PlayerDataManager.Instance.UnbindFromModel(inventoryModel);
        }
    }
}
