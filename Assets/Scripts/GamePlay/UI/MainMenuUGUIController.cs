using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.Inventory.Controller;
using GamePlay.Inventory.Model;
using GamePlay.Role.RoleData;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class MainMenuUGUIController : MonoBehaviour
    {
        [Header("子视图")]
        [SerializeField] TopBarView topBarView;
        [SerializeField] NavBarView navBarView;
        
        [Header("弹窗视图")]
        [SerializeField] LevelPopupView levelPopupView;
        [SerializeField] RoleSelectPopupView roleSelectPopupView;
        
        [Header("玩法模式入口")]
        [SerializeField] Button[] modeButtons;
        
        [Header("导航按钮")]
        [SerializeField] FunctionButtonData[] navButtons;
        
        [Header("玩法数据")]
        [SerializeField] List<ChapterData> levelChapters;

        [Header("角色数据")]
        [SerializeField] RoleRegistry roleRegistry;
        
        [Header("背包管理器")]
        [SerializeField] InventoryController inventoryController;
        [SerializeField] EquipmentController equipmentController;

        void Start()
        {
            InitializeModeButtons();
            InitializeNavBar();
            InitializeLevelPopup();
            InitializeInventory();
        }
        
        private void InitializeModeButtons()
        {
            for (int i = 0; i < modeButtons.Length; i++)
            {
                if (modeButtons[i] == null) continue;

                int index = i;
                
                modeButtons[i].onClick.AddListener(() => OnModeClicked(index));
            }

            Debug.Log($"modeButtons count:{modeButtons.Length}");
        }

        private void InitializeNavBar()
        {
            if (navBarView == null || navButtons == null) return;

            navBarView.OnClicked += OnNavClicked;
            navBarView.Initialize(navButtons);
        }

        private void InitializeLevelPopup()
        {
            if (levelPopupView == null) return;

            levelPopupView.OnLevelClicked += OnLevelClicked;
        }
        
        private void OnModeClicked(int index)
        {
            Debug.Log("mode selected");
            levelPopupView?.Show(levelChapters);
        }

        private void OnLevelClicked(ChapterData chapterData, LevelData level)
        {
            Debug.Log($"关卡选择: 第{chapterData.chapter}章, 第{level.levelIndex + 1}关");
            LevelContext.CurrentChapter = chapterData;
            LevelContext.CurrentLevel = level;

            roleSelectPopupView?.Show();
        }

        private void InitializeInventory()
        {
            InventoryModel model = new InventoryModel();
            inventoryController.Initialize(model);

            equipmentController.Initialize(model);
        }
        
        private void OnNavClicked(int index)
        {
            if (index >= navButtons.Length) return;

            FunctionButtonName nameLabel = navButtons[index].nameLabel;

            if (nameLabel == FunctionButtonName.装备) equipmentController?.ShowEquipmentPopup();
            if (nameLabel == FunctionButtonName.背包) inventoryController?.ShowBackpack();
            //if (nameLabel == FunctionButtonName.强化) roleSelectPopupView?.Show();
            //if (nameLabel == FunctionButtonName.技能) roleSelectPopupView?.Show();
            //if (nameLabel == FunctionButtonName.角色) roleSelectPopupView?.Show();
        }

        void OnDestroy()
        {
            if (navBarView) navBarView.OnClicked -= OnNavClicked;
            if (levelPopupView) levelPopupView.OnLevelClicked -= OnLevelClicked;

            foreach (Button btn in modeButtons)
            {
                if (btn) btn.onClick.RemoveAllListeners();
            }
        }
    }
}
