using System.Collections;
using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.View.ScrollView;
using UnityEngine;

namespace GamePlay.UI.Inventory.View
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryMenu;
        [SerializeField] private ItemSlotScrollView scrollView;

        private InventoryModel inventoryModel;

        [Header("物品详情弹出面板")]
        [SerializeField] private ItemInfoView itemInfoView;
        [SerializeField] private RectTransform backpackPanel;
        [SerializeField] private float spaceBetweenBackpackAndItemInfo;
        [SerializeField] private float slideDuration = 0.25f;
        
        [SerializeField] private UnityEngine.UI.Button closeBtn;
        [SerializeField] private UnityEngine.UI.Button itemInfoViewCloseBtn;
        
        
        private RectTransform itemInfoPanel;
        private Vector2 backpackOriginalPos;
        private bool itemInfoVisible;
        
        public ItemScriptableObject.ItemType CurrentCategoryType { get; private set; }
        
        private ItemSlotView currentSelectedSlot;
        private void Start()
        {
            closeBtn.onClick.AddListener(Hide);
            itemInfoViewCloseBtn.onClick.AddListener(HideItemInfoPanel);
            
            backpackOriginalPos = backpackPanel.anchoredPosition;
            itemInfoPanel = itemInfoView.GetComponent<RectTransform>();
            
            scrollView.OnSlotClicked += OnSlotClicked;
        }

        public void BindInventoryModel(InventoryModel model)
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged -= OnInventoryChanged;
            }

            inventoryModel = model;
            if (inventoryModel != null)
            {
                inventoryModel.OnInventoryChanged += OnInventoryChanged;
            }

            CurrentCategoryType = ItemScriptableObject.ItemType.All;
            
            scrollView.Initialize();
            RefreshDisplay();
        }

        private void OnInventoryChanged(InventoryModel model, ItemScriptableObject.ItemType categoryType)
        {
            if (categoryType != CurrentCategoryType) return;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            IReadOnlyList<ItemDataModel> allItems = inventoryModel.GetItemsByType(CurrentCategoryType);
            List<ItemDataModel> items = new List<ItemDataModel>(allItems.Count);
            foreach (ItemDataModel item in allItems)
            {
                if (!item.IsEquipped) items.Add(item);
            }
            scrollView.SetData(items, inventoryModel.GetItemCapacity(CurrentCategoryType));
        }

        public void SwitchCategory(ItemScriptableObject.ItemType categoryType)
        {
            CurrentCategoryType = categoryType;
            RefreshDisplay();
        }

        public void ClearItemInfo()
        {
            HideItemInfoPanel();
        }

        private void UpdateDescriptionInfo(ItemDataModel currentDataModel)
        {
            if (currentDataModel == null || currentDataModel.IsEmpty())
            {
                ClearItemInfo();
                return;
            }
            
            itemInfoView.Show(currentDataModel);
            ShowItemInfoPanel();
        }
        
        public void Show()
        {
            inventoryMenu.SetActive(true);
            RefreshDisplay();
        }

        public void Hide()
        {
            itemInfoVisible = false;
            inventoryMenu.SetActive(false);
            itemInfoView.Hide();
            backpackPanel.anchoredPosition = backpackOriginalPos;
        }
        
        private void ShowItemInfoPanel()
        {
            if (itemInfoVisible || backpackPanel == null) return;
            itemInfoVisible = true;

            float slideOffset = ((backpackPanel.rect.width - itemInfoView.RectTransform.rect.width) / 2 + itemInfoView.RectTransform.rect.width) / 2 + spaceBetweenBackpackAndItemInfo / 2;
            Vector2 offset =  new Vector2(slideOffset, 0);
            
            StartCoroutine(SlidePanel(backpackPanel, offset));
            StartCoroutine(SlidePanel(itemInfoPanel, -offset));
        }

        private void HideItemInfoPanel()
        {
            if (!itemInfoVisible || backpackPanel == null) return;
            
            itemInfoVisible = false;
            itemInfoView.Hide();
            
            StartCoroutine(SlidePanel(backpackPanel, backpackOriginalPos - backpackPanel.anchoredPosition));
        }

        private IEnumerator SlidePanel(RectTransform panel, Vector2 offset)
        {
            Vector2 startPos = panel.anchoredPosition;
            Vector2 targetPos = panel.anchoredPosition + offset;
            float elapsed = 0;

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                t = t * t * (3f - 2f * t);
                panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            panel.anchoredPosition = targetPos;
        }
        
        private void OnSlotClicked(ItemSlotView slot)
        {
            currentSelectedSlot = slot;

            UpdateDescriptionInfo(slot.currentDataModel);
        }
        
        private void OnDestroy()
        {
            if (inventoryModel != null) inventoryModel.OnInventoryChanged -= OnInventoryChanged;
            
            closeBtn.onClick.RemoveListener(Hide);
        }
    }
}
