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
            inventoryModel.OnItemEquipped += OnItemEquippedRefresh;
            inventoryModel.OnItemUnequipped += OnItemUnequippedRefresh;

            roleInfoView.Init();
            roleInfoView.OnUnequipClicked += OnUnequipItem;
            roleInfoView.OnRoleLabelClicked += OnRoleLabelClicked;

            PopulateRoleLabels();
        }

        void OnDestroy()
        {
            if (inventoryModel != null)
            {
                inventoryModel.OnItemEquipped -= OnItemEquippedRefresh;
                inventoryModel.OnItemUnequipped -= OnItemUnequippedRefresh;
            }

            if (roleInfoView != null)
            {
                roleInfoView.OnUnequipClicked -= OnUnequipItem;
                roleInfoView.OnRoleLabelClicked -= OnRoleLabelClicked;
            }
        }

        /// <summary>
        /// 显示角色信息面板，保留上次选中的角色（首次则选第一个）
        /// </summary>
        public void Show()
        {
            Dictionary<RoleName, RoleSaveData> ownedRoles = PlayerDataManager.Instance.PlayerData.ownedRoles;
            if (curSaveData == null || !ownedRoles.ContainsKey(curRoleName))
            {
                foreach (KeyValuePair<RoleName, RoleSaveData> pair in ownedRoles)
                {
                    SwitchRole(pair.Key, pair.Value);
                    break;
                }
            }

            roleInfoView.ShowPanel();
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
            foreach (RoleProperty property in Enum.GetValues(typeof(RoleProperty)))
            {
                roleInfoView.SetRoleProperty(property, saveData.GetProperty(property));
            }
            
            RefreshEquipmentSlots();

            OnRoleChanged?.Invoke(roleName, saveData);
        }
        
        private void OnItemEquippedRefresh(ItemDataModel item)
        {
            if (item.EquippedByRole == curRoleName) RefreshEquipmentSlots();
        }

        private void OnItemUnequippedRefresh(ItemDataModel item, RoleName previousRole)
        {
            if (previousRole == curRoleName) RefreshEquipmentSlots();
        }
        
        private void RefreshEquipmentSlots()
        {
            roleInfoView.ClearAllEquipmentSlots();
            if (inventoryModel == null) return;

            List<ItemDataModel> equippedItems = inventoryModel.GetEquippedItemsForRole(curRoleName);
            Debug.Log(equippedItems.Count);
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
