using System;
using System.Collections.Generic;
using AssetLoad;
using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.ContainerView;
using UnityEngine;

namespace GamePlay.UI.Inventory.Controller
{
    public class RoleInfoController : MonoBehaviour
    {
        [SerializeField] private RoleInfoView roleInfoView;

        private InventoryModel inventoryModel;
        private RoleName curRoleName;
        private RoleSaveData curSaveData;

        /// <summary>当前选中的角色名</summary>
        public RoleName CurrentRoleName => curRoleName;
        
        /// <summary>角色切换时触发（新角色名, 新角色存档数据）</summary>
        public event Action<RoleName, RoleSaveData> OnRoleChanged;

        public void Initialize(InventoryModel model)
        {
            Debug.Log("RoleInfoController::Initialize");
            inventoryModel = model;
            inventoryModel.OnItemEquipped += OnRefreshEquippedItemAndRoleProperties;
            inventoryModel.OnItemUnequipped += OnRefreshEquippedItemAndRoleProperties;

            roleInfoView.Init();
            roleInfoView.OnUnequipClicked += OnUnequipItem;
            roleInfoView.OnRoleLabelClicked += OnRoleLabelClicked;

            PopulateRoleLabels();
        }

        void OnDestroy()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnItemEquipped -= OnRefreshEquippedItemAndRoleProperties;
                inventoryModel.OnItemUnequipped -= OnRefreshEquippedItemAndRoleProperties;
            }

            if (roleInfoView != null)
            {
                roleInfoView.OnUnequipClicked -= OnUnequipItem;
                roleInfoView.OnRoleLabelClicked -= OnRoleLabelClicked;
            }
        }

        /// <summary>
        /// 显示角色信息面板，保留上次选中的角色（首次则选第一个）
        /// 先激活 GameObject 再设置内容，确保正确布局
        /// </summary>
        public void Show()
        {
            roleInfoView.ShowPanel();

            Dictionary<RoleName, RoleSaveData> ownedRoles = PlayerDataManager.Instance.PlayerData.ownedRoles;
            if (curSaveData == null || !ownedRoles.ContainsKey(curRoleName))
            {
                foreach (KeyValuePair<RoleName, RoleSaveData> pair in ownedRoles)
                {
                    SwitchRole(pair.Key, pair.Value);
                    break;
                }
            }
        }

        public void HidePopup()
        {
            roleInfoView.Hide();
        }
        
        public bool HasEmptyEquipmentSlot()
        {
            return roleInfoView.HasEmptyEquipmentSlot();
        }

        private void OnRoleLabelClicked(RoleName roleName, RoleSaveData saveData)
        {
            SwitchRole(roleName, saveData);
        }

        private void SwitchRole(RoleName roleName, RoleSaveData saveData)
        {
            if (curRoleName == roleName && curSaveData == saveData) return;

            curRoleName = roleName;
            curSaveData = saveData;

            RoleRegistry.RoleEntry? e = GameDataManager.RoleRegistry?.GetRoleEntry(curRoleName);
            roleInfoView.SetRoleBaseInfo(e?.template);

            RefreshRoleProperties();
            RefreshEquipmentSlots();

            OnRoleChanged?.Invoke(roleName, saveData);
        }

        /// <summary>
        /// 从 curSaveData 重新读取属性并刷新 UI 显示
        /// </summary>
        private void RefreshRoleProperties()
        {
            if (curSaveData == null) return;

            foreach (RoleProperty property in Enum.GetValues(typeof(RoleProperty)))
            {
                if (property == RoleProperty.战力 || property == RoleProperty.经验)
                {
                    roleInfoView.SetRoleProperty(property, curSaveData.GetProperty(property), 0f);
                }
                else
                {
                    roleInfoView.SetRoleProperty(property, curSaveData.baseAttributes.GetProperty(property), curSaveData.equipmentBonus.GetProperty(property));
                }
            }

            roleInfoView.ForcePropertiesLayoutRebuild();
        }

        private void OnRefreshEquippedItemAndRoleProperties(ItemDataModel item)
        {
            if (item.EquippedByRole == curRoleName)
            {
                RefreshRoleProperties();
                RefreshEquipmentSlots();
            }
        }
        private void OnRefreshEquippedItemAndRoleProperties(ItemDataModel item, RoleName previousRole)
        {
            if (previousRole == curRoleName)
            {
                RefreshRoleProperties();
                RefreshEquipmentSlots();
            }
        }
        
        private void RefreshEquipmentSlots()
        {
            roleInfoView.ClearAllEquipmentSlots();
            if (inventoryModel == null) return;

            IReadOnlyList<ItemDataModel> equippedItems = inventoryModel.GetEquippedItemsForRole(curRoleName);
            foreach (ItemDataModel item in equippedItems)
            {
                roleInfoView.SetEquipmentInfo(item);
            }
            Debug.Log("RoleInfoController::RefreshEquipmentSlots");
        }
        
        private void OnUnequipItem(ItemDataModel item)
        {
            inventoryModel.UnequipItem(item);
        }

        /// <summary>
        /// 读取已拥有角色并填充角色标签
        /// </summary>
        private void PopulateRoleLabels()
        {
            Dictionary<RoleName, RoleSaveData> ownedRoles = PlayerDataManager.Instance.PlayerData.ownedRoles;
            List<(RoleName, RoleSaveData)> roleList = new List<(RoleName, RoleSaveData)>(ownedRoles.Count);
            foreach (KeyValuePair<RoleName, RoleSaveData> kvp in ownedRoles)
            {
                roleList.Add((kvp.Key, kvp.Value));
            }

            roleInfoView.SetRoleLabels(roleList);
        }

        /// <summary>
        /// 新角色解锁后调用，刷新角色标签
        /// </summary>
        public void RefreshRoleLabels()
        {
            PopulateRoleLabels();
        }
    }
}
