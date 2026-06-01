using System;
using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.UI.Inventory.View.ScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class EquipmentPopupView : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        
        [Header("装备面板")]
        [SerializeField] private EquipmentInfoSlotScrollView equipmentInfoInfoScrollView;
        [SerializeField] private TMP_Text panelText;
        
        public event Action OnClosed;
        public event Action<ItemDataModel> OnSlotFunctionButtonClicked;

        public void Initialize()
        {
            equipmentInfoInfoScrollView.Initialize();
        }
        
        public void Show()
        {
            transform.gameObject.SetActive(true);
        }

        private void Hide()
        {
            transform.gameObject.SetActive(false);
            OnClosed?.Invoke();
        }
        
        void OnEnable()
        {
            if (closeButton) closeButton.onClick.AddListener(Hide);
            
            equipmentInfoInfoScrollView.OnSlotFunctionButtonClicked += OnSlotFunctionButtonClick;
        }
        
        void OnDisable()
        {
            if (closeButton) closeButton.onClick.RemoveListener(Hide);
            equipmentInfoInfoScrollView.OnSlotFunctionButtonClicked -= OnSlotFunctionButtonClick;
        }
        
        private void OnSlotFunctionButtonClick(ItemDataModel dataModel)
        {
            OnSlotFunctionButtonClicked?.Invoke(dataModel);
        }
        
        public void RefreshEquipmentView(FunctionButtonName funBtnNm, IReadOnlyList<ItemDataModel> items)
        {
            panelText.text = funBtnNm.ToString();
            equipmentInfoInfoScrollView.SetFunctionButtonName(funBtnNm);
            equipmentInfoInfoScrollView.SetData(items, items.Count);
        }
    }
}
