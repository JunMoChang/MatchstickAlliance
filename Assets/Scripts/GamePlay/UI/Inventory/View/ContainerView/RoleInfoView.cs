using System;
using System.Collections.Generic;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.SingleView;
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
            public TMPro.TMP_Text baseText; //基础值文本
            public TMPro.TMP_Text bonusText; //装备加成文本
            public GameObject bonusObject; //装备加成Text，控制显隐
        }

        [SerializeField] private Image roleImage;
        [SerializeField] private TMPro.TMP_Text roleNameText;
        [SerializeField] private ItemSlotView[] equipmentSlots;
        [SerializeField] private RolePropertyText[] rolePropertyTexts;
        [SerializeField] private ItemInfoView equipmentInfoView;

        [SerializeField] private GameObject roleContext;
        [SerializeField] private GameObject roleLabelPrefab;
        [SerializeField] private RectTransform propertiesContentRect;

        private Dictionary<RoleProperty, RolePropertyText> rolePropertyDic;
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

            rolePropertyDic = new Dictionary<RoleProperty, RolePropertyText>(rolePropertyTexts.Length);
            foreach (RolePropertyText property in rolePropertyTexts)
            {
                rolePropertyDic.Add(property.property, property);
                
                if (property.bonusObject != null) property.bonusObject.SetActive(false);
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
            foreach ((RoleName roleName, RoleSaveData saveData) in roles)
            {
                GameObject label = Instantiate(roleLabelPrefab, roleContext.transform);

                TMPro.TMP_Text nameText = label.GetComponentInChildren<TMPro.TMP_Text>();
                if (nameText != null) nameText.text = $"{roleName} \n Lv:{saveData.roleLevel}";

                Button btn = label.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => OnRoleLabelClicked?.Invoke(roleName, saveData));
                

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
        public void SetRoleProperty(RoleProperty property, float baseValue, float equipBonus)
        {
            if (!rolePropertyDic.TryGetValue(property, out RolePropertyText text)) return;

            bool isCrit = property == RoleProperty.暴击;
            string format = isCrit ? "P2" : "F0";
            
            text.baseText.text = $"{property}: {baseValue.ToString(format)}";

            if (text.bonusText == null) return;
            
            if (equipBonus != 0f)
            {
                text.bonusText.text = $"+{equipBonus.ToString(format)}";

                if (text.bonusObject != null) text.bonusObject.SetActive(true);
            }
            else
            {
                if (text.bonusObject != null) text.bonusObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 解决首次激活时 ContentSizeFitter+HLG 排列错乱
        /// </summary>
        public void ForcePropertiesLayoutRebuild()
        {
            foreach (RolePropertyText text in rolePropertyTexts)
            {
                if (text.baseText != null) text.baseText.ForceMeshUpdate();
                
                if (text.bonusText != null) text.bonusText.ForceMeshUpdate();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(propertiesContentRect);
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
