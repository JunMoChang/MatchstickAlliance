using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.UI;
using UnityEngine;

namespace GamePlay.Inventory.Controller
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private RoleInfoView roleInfoView;
        [SerializeField] private EquipmentPopupView equipmentPopupView;
        
        private InventoryModel inventoryModel;
        private FunctionButtonName curFunBtnNm;

        public void Initialize(InventoryModel _inventoryModel)
        {
            inventoryModel = _inventoryModel;
            inventoryModel.OnInventoryChanged += OnModelChanged;

            equipmentPopupView.Initialize();
            equipmentPopupView.OnSlotFunctionButtonClicked += OnSlotFunctionButtonClicked;
            equipmentPopupView.OnClosed += OnEquipmentPopupClosed;

            roleInfoView.Init();
            roleInfoView.OnUnequipClicked += OnRoleInfoUnequipClicked;

            RefreshView();
        }

        public void ShowEquipmentPopup()
        {
            roleInfoView.Show();
            equipmentPopupView.Show();
            RefreshView();
        }

        private void OnEquipmentPopupClosed()
        {
            roleInfoView.Hide();
        }

        private void OnRoleInfoUnequipClicked(ItemDataModel itemData)
        {
            inventoryModel.UnequipItem(itemData);
        }
        
        public void OnFunctionButtonNameChanged(FunctionButtonName funBtnNm)
        {
            if (curFunBtnNm == funBtnNm) return;
            
            curFunBtnNm = funBtnNm;
            RefreshView();
        }
        
        private void OnSlotFunctionButtonClicked(ItemDataModel itemData)
        {
            switch (curFunBtnNm)
            {
                case FunctionButtonName.装备:
                    if (itemData.IsEquipped) break;

                    if (itemData.StorageItemQuantity > 1)
                    {
                        if (!roleInfoView.HasEmptyEquipmentSlot()) break;

                        ItemDataModel equippedItem = inventoryModel.SplitEquipItem(itemData);
                        roleInfoView.SetEquipmentInfo(equippedItem);
                    }
                    else
                    {
                        if (!roleInfoView.SetEquipmentInfo(itemData)) break;
                        inventoryModel.EquipItem(itemData);
                    }
                    break;
                case FunctionButtonName.强化:
                    break;
            }
        }
        
        private void OnModelChanged(InventoryModel _, ItemScriptableObject.ItemType __)
        {
            RefreshView();
        }

        private void RefreshView()
        {
            IReadOnlyList<ItemDataModel> items = curFunBtnNm == FunctionButtonName.装备
                ? (IReadOnlyList<ItemDataModel>)inventoryModel.GetUnequippedEquipment()
                : inventoryModel.GetEquippedItems();
            
            equipmentPopupView.RefreshEquipmentView(curFunBtnNm, items);
        }
    }
}