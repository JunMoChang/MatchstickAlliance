using System;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI.Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class ItemInfoView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemQuantityText;
        [SerializeField] private TMP_Text itemPriceText;
        [SerializeField] private TMP_Text itemPropertiesText;

        [SerializeField] private Button deleteButton;
        [SerializeField] private Button addButton;
        [SerializeField] private Button maxButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button unequipButton;
        public RectTransform RectTransform { get; private set; }

        public event Action<ItemDataModel> OnUnequipClicked;

        private ItemDataModel currentDataModel;
        private Vector2 originalPosition;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            originalPosition =  RectTransform.anchoredPosition;
        }

        void OnEnable()
        {
            if (unequipButton) unequipButton.onClick.AddListener(OnUnequipButtonClick);
        }

        void OnDisable()
        {
            if (unequipButton) unequipButton.onClick.RemoveListener(OnUnequipButtonClick);
        }

        public void Show(ItemDataModel dataModel)
        {
            if (dataModel == null || dataModel.IsEmpty())
            {
                Hide();
                return;
            }

            currentDataModel = dataModel;
            ItemScriptableObject so = dataModel.ItemSo;

            itemImage.sprite = so.GetItemSprite(dataModel.ItemRarity);
            itemNameText.text = so.itemName.ToString();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (ItemScriptableObject.ItemProperty prop in so.itemProperties)
            {
                sb.AppendLine($"{prop.itemPropertyType.ToString()}: {prop.baseValue}");
            }
            itemPropertiesText.text = sb.ToString();

            if (unequipButton != null) unequipButton.gameObject.SetActive(dataModel.IsEquipped);

            panel.SetActive(true);
        }

        public void Hide()
        {
            panel.SetActive(false);

            if(RectTransform != null) RectTransform.anchoredPosition = originalPosition;
        }

        private void OnUnequipButtonClick()
        {
            if (currentDataModel == null) return;
            OnUnequipClicked?.Invoke(currentDataModel);
            Hide();
        }
    }
}
