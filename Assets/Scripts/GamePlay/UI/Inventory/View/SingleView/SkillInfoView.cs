using System;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class SkillInfoView : MonoBehaviour, ISlotView
    {
        [SerializeField] private Image skillIcon;
        [SerializeField] private TMP_Text skillInfoText;
        [SerializeField] private TMP_Text enhancedPayText;
        [SerializeField] private Button enhanceButton;

        public int SkillIndex { get; private set; }
        public RectTransform RectTransform { get; private set; }

        public event Action<int> OnEnhanceClicked;

        public void Init()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            if (enhanceButton != null) enhanceButton.onClick.AddListener(HandleEnhanceClick);
        }

        void OnDisable()
        {
            if (enhanceButton != null) enhanceButton.onClick.RemoveListener(HandleEnhanceClick);
        }

        void HandleEnhanceClick()
        {
            OnEnhanceClicked?.Invoke(SkillIndex);
        }

        public void SetData(RoleBaseData.SkillData config, SkillSaveData saveData, int skillIndex, int enhanceCost)
        {
            SkillIndex = skillIndex;

            skillIcon.sprite = config.icon;
            skillInfoText.text = $"Lv.{saveData.level} \n 基础攻击: {config.baseDamage:P0} 伤害: {saveData.damage}";
            enhancedPayText.text = enhanceCost.ToString();
            Debug.Log($"{SkillIndex} {config.icon}");
        }
    }
}
