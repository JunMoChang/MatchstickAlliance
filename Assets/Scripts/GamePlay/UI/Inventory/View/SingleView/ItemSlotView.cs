using System;
using GamePlay.UI.Inventory.Model;
using UnityEngine;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class ItemSlotView : MonoBehaviour, ISlotView
    {
        [Header("UI组件")]
        [SerializeField] private UnityEngine.UI.Image itemImage;
        [SerializeField] private Sprite nullSprite;
        [SerializeField] private TMPro.TMP_Text itemQuantityText;
        [SerializeField] private GameObject enhanceBg;
        private TMPro.TMP_Text enhancementLevelText;
        
        public RectTransform RectTransform { get; private set; }
        
        [NonSerialized] public ItemDataModel currentDataModel;
        
        public event Action<ItemSlotView> OnClicked;

        public void NotifyClicked()
        {
            OnClicked?.Invoke(this);
        }
        
        public void Init()
        {
            RectTransform = GetComponent<RectTransform>();
            
            enhancementLevelText = enhanceBg.GetComponentInChildren<TMPro.TMP_Text>();
        }
        
        public void SetData(ItemDataModel dataModel)
        {
            if (currentDataModel != null)
            {
                currentDataModel.OnDataChanged -= OnModelDataChanged;
            }
        
            currentDataModel = dataModel;
            
            if (currentDataModel != null)
            {
                currentDataModel.OnDataChanged += OnModelDataChanged;
            }
            
            UpdateDataDisplay();
        }
        
        private void OnModelDataChanged(ItemDataModel dataModel)
        {
            UpdateDataDisplay();
        }
        private void UpdateDataDisplay()
        {
            if (currentDataModel == null || currentDataModel.IsEmpty())
            {
                if (itemQuantityText != null) itemQuantityText.enabled = false;
                
                enhanceBg?.SetActive(false);
                if (enhancementLevelText != null) enhancementLevelText.enabled = false;
                
                itemImage.sprite = nullSprite;
            }
            else
            {
                itemImage.sprite = currentDataModel.ItemSo.GetItemSprite(currentDataModel.ItemRarity);
                
                if (itemQuantityText != null)
                {
                    itemQuantityText.text = currentDataModel.StorageItemQuantity.ToString();
                    itemQuantityText.enabled = true;
                }
                
                bool enhanced = currentDataModel.EnhancementLevel > 0;
                if (enhanced)
                {
                    enhanceBg?.SetActive(true);
                    if (enhancementLevelText != null)
                    {
                        enhancementLevelText.enabled = true;
                        enhancementLevelText.text = $"+{currentDataModel.EnhancementLevel}";
                    }
                }
                else
                {
                    enhanceBg?.SetActive(false);
                    if (enhancementLevelText != null) enhancementLevelText.enabled = false;
                }
            }
        }
    }
}