using System.Collections.Generic;
using AssetLoad;
using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using GamePlay.UI.Inventory.View.SingleView;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.Controller
{
    public class SkillInfoController : MonoBehaviour
    {
        [SerializeField] private SkillInfoView[] skillViews;

        [SerializeField] private GameObject skillInfoPanel;
        [SerializeField] private Button closeButton;
        private RoleBaseData currentTemplate;
        private RoleSaveData currentSaveData;

        private RoleInfoController roleInfoController;

        public void Initialize(RoleInfoController roleInfoCtrl)
        {
            Debug.Log("SkillInfoController::Initialize");
            roleInfoController = roleInfoCtrl;
            roleInfoController.OnRoleChanged += OnExternalRoleChanged;

            foreach (SkillInfoView view in skillViews)
            {
                view.Init();
                view.OnEnhanceClicked += OnEnhanceSkill;
            }

            closeButton.onClick.AddListener(HidePopup);
        }

        void OnDestroy()
        {
            if (roleInfoController != null)
                roleInfoController.OnRoleChanged -= OnExternalRoleChanged;

            foreach (SkillInfoView view in skillViews)
            {
                if (view != null) view.OnEnhanceClicked -= OnEnhanceSkill;
            }

            closeButton.onClick.RemoveListener(HidePopup);
        }
        
        /// <summary>
        /// 显示技能面板, 保留上次选中的角色(首次则选第一个）
        /// </summary>
        public void Show()
        {
            Dictionary<RoleName, RoleSaveData> saveDataDic = PlayerDataManager.Instance.PlayerData.ownedRoles;
            if(currentSaveData == null || currentTemplate == null || saveDataDic.ContainsKey(currentSaveData.roleName))
            {
                foreach (RoleSaveData saveData in saveDataDic.Values)
                {
                    SwitchRoleSkill(saveData);
                    break;
                }
            }
            
            skillInfoPanel.SetActive(true);
        }

        public void HidePopup()
        {
            skillInfoPanel.SetActive(false);
            roleInfoController.HidePopup();
        }

        private void SwitchRoleSkill(RoleSaveData saveData)
        {
            currentSaveData = saveData;
            currentTemplate = GameDataManager.RoleRegistry?.GetRoleEntry(saveData.roleName)?.template;
            RefreshAllSkills();
        }
        
        private void OnExternalRoleChanged(RoleName roleName, RoleSaveData saveData)
        {
            if (skillInfoPanel.activeSelf) SwitchRoleSkill(saveData);
        }

        private void RefreshAllSkills()
        {
            if (currentTemplate == null || currentSaveData == null) return;
            
            int skillCount = currentTemplate.skillsBaseData.Length;
            for (int i = 0; i < skillViews.Length; i++)
            {
                if (i < skillCount)
                {
                    
                    RoleBaseData.SkillData config = currentTemplate.skillsBaseData[i];
                    SkillSaveData save = currentSaveData.skillsData[i];

                    int cost = CalcEnhanceCost(save.level);
                    
                    skillViews[i].SetData(config, save, i, cost);
                    skillViews[i].gameObject.SetActive(true);
                }
                else
                {
                    skillViews[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnEnhanceSkill(int skillIndex)
        {
            int cost = CalcEnhanceCost(currentSaveData.skillsData[skillIndex].level);
            if (!PlayerDataManager.Instance.SpendGold(cost)) return;
            
            currentTemplate.UpdateSkillData(currentSaveData, skillIndex, 1);
            PlayerDataManager.Instance.Save();
            RefreshAllSkills();
        }

        private static int CalcEnhanceCost(int currentLevel)
        {
            return 100 + currentLevel * 50;
        }
    }
}
