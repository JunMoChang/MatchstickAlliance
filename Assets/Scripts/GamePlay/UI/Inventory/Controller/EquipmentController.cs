using System.Collections.Generic;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.ContainerView;
using UnityEngine;

namespace GamePlay.UI.Inventory.Controller
{
    public class EquipmentController : MonoBehaviour
    {
        [SerializeField] private RoleInfoController roleInfoController;
        [SerializeField] private EquipmentPopupView equipmentPopupView;

        private InventoryModel inventoryModel;
        private FunctionButtonName curFunBtnNm;

        public void Initialize(InventoryModel _inventoryModel)
        {
            Debug.Log("EquipmentController::Initialize");
            inventoryModel = _inventoryModel;

            inventoryModel.OnItemEquipped += OnRefreshUnEquippedItem;
            inventoryModel.OnItemUnequipped += OnRefreshUnEquippedItem;

            equipmentPopupView.Initialize();
            equipmentPopupView.OnSlotFunctionButtonClicked += OnSlotFunctionButtonClicked;
            equipmentPopupView.OnClosed += OnEquipmentPopupClosed;

            RefreshView();
        }

        void OnDestroy()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnItemEquipped -= OnRefreshUnEquippedItem;
                inventoryModel.OnItemUnequipped -= OnRefreshUnEquippedItem;
            }

            if (equipmentPopupView != null)
            {
                equipmentPopupView.OnSlotFunctionButtonClicked -= OnSlotFunctionButtonClicked;
                equipmentPopupView.OnClosed -= OnEquipmentPopupClosed;
            }
        }

        private void OnRefreshUnEquippedItem(ItemDataModel item)
        {
            RefreshView();
        }
        private void OnRefreshUnEquippedItem(ItemDataModel item, Role.RoleData.RoleName previousRole)
        {
            RefreshView();
        }

        public void ShowEquipmentPopup()
        {
            roleInfoController.Show();
            equipmentPopupView.Show();
            RefreshView();
        }

        private void OnEquipmentPopupClosed()
        {
            roleInfoController.HidePopup();
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
                        if (!roleInfoController.HasEmptyEquipmentSlot()) break;

                        inventoryModel.SplitEquipItem(itemData, roleInfoController.CurrentRoleName);
                    }
                    else
                    {
                        if (!roleInfoController.HasEmptyEquipmentSlot()) break;
                        inventoryModel.EquipItem(itemData, roleInfoController.CurrentRoleName);
                    }
                    break;
                case FunctionButtonName.强化:
                    break;
            }
        }

        public void HidePopup()
        {
            equipmentPopupView.gameObject.SetActive(false);
            roleInfoController.HidePopup();
        }

        private void RefreshView()
        {
            IReadOnlyList<ItemDataModel> items = curFunBtnNm == FunctionButtonName.装备
                ? (IReadOnlyList<ItemDataModel>)inventoryModel.GetUnequippedEquipment()
                : inventoryModel.GetEquippedItemsForRole(roleInfoController.CurrentRoleName);

            equipmentPopupView.RefreshEquipmentView(curFunBtnNm, items);
        }
    }
}
