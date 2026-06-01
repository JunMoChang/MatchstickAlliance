using System;
using System.Collections.Generic;
using GamePlay.Inventory.Model;
using GamePlay.PlayerDataHandle;
using GamePlay.Role;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.View;
using GamePlay.UI.Inventory.View.SingleView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class RoleInfoView : MonoBehaviour
    {
        [Serializable]
        private struct RolePropertyText
        {
            public RoleProperty property;
            public TMP_Text text;
        }

        [SerializeField] private TMP_Text roleNameText;
        [SerializeField] private ItemSlotView[] equipmentSlots;
        [SerializeField] private RolePropertyText[] rolePropertyTexts;
        [SerializeField] private ItemInfoView equipmentInfoView;

        [SerializeField] private GameObject roleContext;
        [SerializeField] private GameObject roleLabelPrefab;
        [SerializeField] private RoleRegistry roleRegistry;

        private Dictionary<RoleProperty, TMP_Text> rolePropertyDic;
        private RoleSaveData currentSaveData;
        private readonly List<GameObject> roleLabelInstances = new ();

        public event Action<ItemDataModel> OnUnequipClicked;

        public void Init()
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                slotView.Init();
                slotView.OnClicked += OnSlotClicked;
            }

            equipmentInfoView.OnUnequipClicked += OnEquipmentInfoUnequipClicked;

            rolePropertyDic = new Dictionary<RoleProperty, TMP_Text>(rolePropertyTexts.Length);
            foreach (RolePropertyText property in rolePropertyTexts)
            {
                rolePropertyDic.Add(property.property, property.text);
            }

            BuildRoleLabels();
        }

        void OnDestroy()
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                slotView.OnClicked -= OnSlotClicked;
            }

            if (equipmentInfoView) equipmentInfoView.OnUnequipClicked -= OnEquipmentInfoUnequipClicked;

            ClearRoleLabels();
        }

        public void Show()
        {
            Dictionary<RoleName, RoleSaveData> ownedRoles = PlayerDataManager.Instance.PlayerData.ownedRoles;
            foreach (KeyValuePair<RoleName, RoleSaveData> kvp in ownedRoles)
            {
                SwitchRole(kvp.Key, kvp.Value);
                break;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RefreshRoleLabels()
        {
            BuildRoleLabels();
        }

        private void BuildRoleLabels()
        {
            ClearRoleLabels();
            Dictionary<RoleName, RoleSaveData> ownedRoles = PlayerDataManager.Instance.PlayerData.ownedRoles;
            foreach (KeyValuePair<RoleName, RoleSaveData> kvp in ownedRoles)
            {
                GameObject label = Instantiate(roleLabelPrefab, roleContext.transform);

                TMP_Text nameText = label.GetComponentInChildren<TMP_Text>();
                if (nameText != null) nameText.text = $"{kvp.Key} \n {kvp.Value.roleLevel}";

                Button btn = label.GetComponent<Button>();
                if (btn)
                {
                    RoleName rn = kvp.Key;
                    RoleSaveData sd = kvp.Value;
                    btn.onClick.AddListener(() => SwitchRole(rn, sd));
                }

                roleLabelInstances.Add(label);
            }
        }

        private void SwitchRole(RoleName roleName, RoleSaveData saveData)
        {
            currentSaveData = saveData;
            roleNameText.text = roleName.ToString();
            RefreshAllProperties();
        }

        private void ClearRoleLabels()
        {
            foreach (GameObject label in roleLabelInstances)
            {
                if (label == null) continue;
                Button btn = label.GetComponent<Button>();
                if (btn) btn.onClick.RemoveAllListeners();
                Destroy(label);
            }
            roleLabelInstances.Clear();
        }

        private void OnEquipmentInfoUnequipClicked(ItemDataModel dataModel)
        {
            ClearEquipmentSlotInfo(dataModel);
            OnUnequipClicked?.Invoke(dataModel);
        }

        private void OnSlotClicked(ItemSlotView slot)
        {
            UpdateItemDescriptionInfo(slot.currentDataModel);
        }

        private void UpdateItemDescriptionInfo(ItemDataModel currentDataModel)
        {
            equipmentInfoView.Show(currentDataModel);
        }

        private void RefreshAllProperties()
        {
            foreach (var kvp in rolePropertyDic)
            {
                UpdateRolePropertyDisplay(kvp.Key);
            }
        }

        private void UpdateRolePropertyDisplay(RoleProperty property)
        {
            if (rolePropertyDic.TryGetValue(property, out TMP_Text text))
            {
                float value = currentSaveData.GetProperty(property);
                text.text = $"{property}: {value}";
            }
        }

        public bool SetEquipmentInfo(ItemDataModel currentDataModel)
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                if (slotView.currentDataModel != null) continue;

                slotView.SetData(currentDataModel);
                return true;
            }
            return false;
        }

        public bool HasEmptyEquipmentSlot()
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                if (slotView.currentDataModel == null) return true;
            }
            return false;
        }

        private void ClearEquipmentSlotInfo(ItemDataModel dataModel)
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                if (slotView.currentDataModel == dataModel)
                {
                    slotView.SetData(null);
                    return;
                }
            }
        }
    }
}
