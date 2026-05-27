using System;
using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.Inventory.View;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class EquipmentPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Button closeButton;

        [Header("标签页")]
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] tabPanels;
        
        [Header("装备面板")]
        [SerializeField] private EquipmentInfoSlotsScrollView equipmentInfoInfoScrollView;
        
        public event Action OnClosed;
        public event Action<FunctionButtonName> OnFunctionButtonNameChanged;
        public event Action<ItemDataModel> OnSlotFunctionButtonClicked;
        
        public void Show()
        {
            rootPanel.SetActive(true);
        }

        public void Hide()
        {
            rootPanel.SetActive(false);
            OnClosed?.Invoke();
        }
        
        void OnEnable()
        {
            if (closeButton) closeButton.onClick.AddListener(Hide);
            
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i;
                tabButtons[i]?.onClick.AddListener(() =>
                {
                    OnFunctionButtonNameChanged?.Invoke((FunctionButtonName)index);
                });
            }
            
            equipmentInfoInfoScrollView.OnSlotFunctionButtonClicked += OnSlotFunctionButtonClick;
        }
        
        void OnDisable()
        {
            if (closeButton) closeButton.onClick.RemoveListener(Hide);
            equipmentInfoInfoScrollView.OnSlotFunctionButtonClicked -= OnSlotFunctionButtonClick;
            foreach (Button btn in tabButtons)
            {
                if (btn) btn.onClick.RemoveAllListeners();
            }
        }

        private void OnSlotFunctionButtonClick(ItemDataModel dataModel)
        {
            OnSlotFunctionButtonClicked?.Invoke(dataModel);
        }
        public void RefreshEquipmentView(FunctionButtonName funBtnNm, IReadOnlyList<ItemDataModel> items)
        {
            equipmentInfoInfoScrollView.SetFunctionButtonName(funBtnNm);
            equipmentInfoInfoScrollView.SetData(items, items.Count);
        }
    }
}
