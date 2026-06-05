using System;
using System.Collections.Generic;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.SingleView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.ContainerView
{
    public class RoleInfoView : MonoBehaviour
    {
        [Serializable]
        private struct RolePropertyText
        {
            public RoleProperty property;
            public TMP_Text text;
        }
    
        [SerializeField] private Image roleImage;
        [SerializeField] private TMP_Text roleNameText;
        [SerializeField] private ItemSlotView[] equipmentSlots;
        [SerializeField] private RolePropertyText[] rolePropertyTexts;
        [SerializeField] private ItemInfoView equipmentInfoView;

        [SerializeField] private GameObject roleContext;
        [SerializeField] private GameObject roleLabelPrefab;

        private Dictionary<RoleProperty, TMP_Text> rolePropertyDic;
        private readonly List<GameObject> roleLabelInstances = new();

        public event Action<ItemDataModel> OnUnequipClicked;
        public event Action<RoleName, RoleSaveData> OnRoleLabelClicked;

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

        public void ShowPanel()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void SetRoleLabels(IReadOnlyList<(RoleName name, RoleSaveData saveData)> roles)
        {
            ClearRoleLabels();
            foreach ((RoleName name, RoleSaveData saveData) in roles)
            {
                GameObject label = Instantiate(roleLabelPrefab, roleContext.transform);

                TMP_Text nameText = label.GetComponentInChildren<TMP_Text>();
                if (nameText != null) nameText.text = $"{name} \n Lv:{saveData.roleLevel}";

                Button btn = label.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => OnRoleLabelClicked?.Invoke(name, saveData));
                

                roleLabelInstances.Add(label);
            }
        }
        
        public void SetRoleBaseInfo(RoleBaseData roleData)
        {
            if (roleData == null) return;
            
            roleNameText.text = roleData.roleName.ToString();
            roleImage.sprite = roleData.exhibitionIcon;
        }

        /// <summary>
        /// 设置角色单个属性显示
        /// </summary>
        public void SetRoleProperty(RoleProperty property, float value)
        {
            if (rolePropertyDic.TryGetValue(property, out TMP_Text text))
            {
                text.text = $"{property}: {value}";
            }
        }
        
        public void ClearAllEquipmentSlots()
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                slotView.SetData(null);
            }
        }

        public void SetEquipmentInfo(ItemDataModel currentDataModel)
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                if (slotView.currentDataModel != null) continue;

                slotView.SetData(currentDataModel);
                Debug.Log("RoleInfoView::SetEquipmentInfo");
                return;
            }
        }

        public bool HasEmptyEquipmentSlot()
        {
            foreach (ItemSlotView slotView in equipmentSlots)
            {
                if (slotView.currentDataModel == null) return true;
            }
            return false;
        }

        private void ClearRoleLabels()
        {
            foreach (GameObject label in roleLabelInstances)
            {
                if (label == null) continue;
                Button btn = label.GetComponent<Button>();
                if (btn != null) btn.onClick.RemoveAllListeners();
                Destroy(label);
            }
            roleLabelInstances.Clear();
        }

        private void OnEquipmentInfoUnequipClicked(ItemDataModel dataModel)
        {
            ClearClickedEquipmentSlotInfo(dataModel);
            OnUnequipClicked?.Invoke(dataModel);
        }

        private void OnSlotClicked(ItemSlotView slot)
        {
            UpdateEquipmentInfo(slot.currentDataModel);
        }

        private void UpdateEquipmentInfo(ItemDataModel currentDataModel)
        {
            equipmentInfoView.Show(currentDataModel);
        }

        private void ClearClickedEquipmentSlotInfo(ItemDataModel dataModel)
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
