using System;
using System.Text;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.Model;
using UnityEngine;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class EquipmentInfoSlotView : MonoBehaviour, ISlotView
    {
        [Header("UI组件")]
        [SerializeField] private UnityEngine.UI.Image itemImage;
        [SerializeField] private TMPro.TMP_Text itemQuantityText;
        [SerializeField] private TMPro.TMP_Text propertiesText;
        [SerializeField] private UnityEngine.UI.Button functionButton;
        [SerializeField] private Sprite nullSprite;
        
        private TMPro.TMP_Text functionNameText;
        public RectTransform RectTransform { get; private set; }
        
        public event Action<ItemDataModel> OnFunctionButtonClicked;
        public void Init()
        {
            RectTransform = GetComponent<RectTransform>();
            functionNameText = functionButton.GetComponentInChildren<TMPro.TMP_Text>();
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
                sb.AppendLine($"{property.itemPropertyType.ToString()}:{property.baseValue}");
            }
            propertiesText.text = sb.ToString();
            
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
            functionNameText.text = string.Empty;
            functionButton.onClick.RemoveAllListeners();
        }
    }
}