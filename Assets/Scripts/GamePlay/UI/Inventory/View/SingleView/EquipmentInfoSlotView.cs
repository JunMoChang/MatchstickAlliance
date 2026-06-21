using System;
using System.Text;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.ScriptObjects;
using UnityEngine;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class EquipmentInfoSlotView : MonoBehaviour, ISlotView
    {
        [Header("UI组件")]
        [SerializeField] private UnityEngine.UI.Image itemImage;
        [SerializeField] private TMPro.TMP_Text itemQuantityText;
        [SerializeField] private TMPro.TMP_Text propertiesText;
        [SerializeField] private GameObject enhanceBg;
        [SerializeField] private UnityEngine.UI.Button functionButton;
        [SerializeField] private Sprite nullSprite;
        
        private TMPro.TMP_Text enhancementLevelText;
        private TMPro.TMP_Text functionNameText;
        public RectTransform RectTransform { get; private set; }
        
        public event Action<ItemDataModel> OnFunctionButtonClicked;
        public void Init()
        {
            RectTransform = GetComponent<RectTransform>();
            functionNameText = functionButton.GetComponentInChildren<TMPro.TMP_Text>();
            enhancementLevelText = enhanceBg.GetComponentInChildren<TMPro.TMP_Text>();
        }
        
        public void SetData(ItemDataModel data, FunctionButtonName funBtnNm)
        {
            if (data == null || data.IsEmpty())
            {
                ClearData();
                return;
            }

            itemImage.sprite = data.ItemSo.GetItemSprite(data.ItemRarity);
            itemQuantityText.text = data.StorageItemQuantity.ToString();

            StringBuilder sb = new StringBuilder();
            foreach (ItemScriptableObject.ItemProperty property in data.ItemSo.itemProperties)
            {
                float currentValue = data.CachedBonus.GetProperty(property.itemPropertyType);
                bool isCrit = property.itemPropertyType == ItemScriptableObject.ItemPropertyType.Critical;
                string format = isCrit ? "P2" : "F0";
                sb.AppendLine($"{property.itemPropertyType.ToString()}:{currentValue.ToString(format)}");
            }
            propertiesText.text = sb.ToString();
            
            bool enhanced = data.EnhancementLevel > 0;
            if (enhanced && enhanceBg != null && enhancementLevelText != null)
            {
                enhanceBg.SetActive(true);
                enhancementLevelText.enabled = true;
                enhancementLevelText.text = $"+{data.EnhancementLevel}";
            }
            else
            {
                enhanceBg?.SetActive(false);
                if (enhancementLevelText != null) enhancementLevelText.enabled = false;
            }

            SwitchFunctionName(funBtnNm);
            functionButton.onClick.RemoveAllListeners();
            functionButton.onClick.AddListener(() => OnFunctionButtonClicked?.Invoke(data));
        }
        
        void SwitchFunctionName(FunctionButtonName functionButtonName)
        {
            switch (functionButtonName)
            {
                case FunctionButtonName.装备: 
                    functionNameText.text = nameof(FunctionButtonName.装备); break;
                case FunctionButtonName.强化: 
                    functionNameText.text = nameof(FunctionButtonName.强化); break;
            }
        }
        
        private void ClearData()
        {
            itemImage.sprite = nullSprite;
            itemQuantityText.text = string.Empty;
            propertiesText.text = string.Empty;
            
            enhanceBg?.SetActive(false);
            if (enhancementLevelText != null)
            {
                enhancementLevelText.text = string.Empty;
                enhancementLevelText.enabled = false;
            }
            
            functionNameText.text = string.Empty;
            functionButton.onClick.RemoveAllListeners();
        }
    }
}