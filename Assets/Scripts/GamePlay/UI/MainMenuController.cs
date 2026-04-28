using System;
using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.Role;
using GamePlay.Scene;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace GamePlay.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI 绑定")]
        [SerializeField] UIDocument uiDocument;
        [SerializeField] VisualTreeAsset gameModeTemplate;

        [Header("背景图片")]
        [SerializeField] Sprite backgroundSprite;

        [Header("玩法数据")]
        [SerializeField] List<GameModeData> gameModes = new();
        [SerializeField] private List<ChapterData> levelChapters;
        [SerializeField] private LevelContext levelContext;
        
        [Header("导航按钮")]
        [SerializeField] List<NavButtonData> navButtons = new();

        [SerializeField] RoleRegistry roleRegistry;
        
        private VisualElement mainRoot;
        private VisualElement cardTrack;
        private VisualElement levelPopup;
        private VisualElement equipmentPopup;
        private VisualElement roleSelectPopup;
        private VisualElement currentActive;
        
        private VisualElement levelBg;
        private VisualElement levelContent;
        private VisualElement levelDots;
        private Label chapterLabel;
        private Button closeBtn;
        private Button btnPrev;
        private Button btnNext;
        
        private int currentChapter;
        private int currentLevel;
        
        private void OnEnable()
        {
            mainRoot = uiDocument.rootVisualElement;
        
            cardTrack = SafeQuery<VisualElement>("card-track");
            levelPopup = SafeQuery<VisualElement>("level-popup");
            equipmentPopup =  SafeQuery<VisualElement>("equipment-popup");
            roleSelectPopup = SafeQuery<VisualElement>("roleSelect-popup");
            if (backgroundSprite != null)
            {
                VisualElement bg = mainRoot.Q<VisualElement>("background");
                if (bg != null) bg.style.backgroundImage = new StyleBackground(backgroundSprite);
            }
            
            BuildCards();
            BuildNavBar();
            BuildLevel();
            BuildEquipmentPopup();
            BuildHeroes(roleRegistry.entries);
        }

        private void BuildCards()
        {
            cardTrack.Clear();
        
            for (int i = 0; i < gameModes.Count; i++)
            {
                GameModeData modeData  = gameModes[i];
            
                TemplateContainer clone = gameModeTemplate.CloneTree();
                cardTrack.Add(clone);
            
                VisualElement card = clone.Q<VisualElement>("card-root");
                if (card == null)
                {
                    Debug.LogError($"卡片{i + 1}: 找不到 name='card-root'，请检查 GameCard.uxml");
                    continue;
                }
            
                VisualElement cardBg = card.Q<VisualElement>("card-bg");
                if (cardBg != null && modeData.cardImage != null) cardBg.style.backgroundImage = new StyleBackground(modeData.cardImage);
            
                card.RegisterCallback<ClickEvent>(_ =>
                {
                    currentActive?.RemoveFromClassList("game-card--active");
                
                    card.AddToClassList("game-card--active");
                    currentActive = card;
            
                    OnEnterMode(modeData);
                });

                card.RegisterCallback<PointerDownEvent>(_ => card.style.scale = new Scale(new Vector3(0.97f, 0.97f, 1f)));
                card.RegisterCallback<PointerUpEvent>(_ => card.style.scale = StyleKeyword.Null);
                card.RegisterCallback<ClickEvent>(_ => ShowLevelPopup());
            } 
        }
        
        private void BuildLevel()
        {
            levelPopup.style.display = DisplayStyle.None;

            levelBg = levelPopup.Q<VisualElement>("level-bg");
            levelContent = levelBg.Q<VisualElement>("level-content");
            chapterLabel =  levelBg.Q<Label>("level-chapter-label");
            levelDots = levelBg.Q<VisualElement>("level-dots");
            closeBtn = levelBg.Q<Button>("close-btn");
            btnPrev = levelBg.Q<Button>("btn-prev");
            btnNext = levelBg.Q<Button>("btn-next");
            
            List<Button> levelButtons = levelContent.Query<Button>("level-icon").ToList();
            for (int i = 0; i < levelChapters[0].levelData.Length; i++)
            {
                int index = i;
                levelButtons[i].RegisterCallback<ClickEvent>(_ =>
                {
                    SetRoleSelectPopup(DisplayStyle.Flex);
                    currentLevel = levelChapters[0].levelData[index].levelIndex;
                });
            }
            
            levelPopup.RegisterCallback<ClickEvent>(_ => HideLevelPopup());
            levelBg.RegisterCallback<ClickEvent>(e => e.StopPropagation());
            closeBtn.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                HideLevelPopup();
            });
            btnPrev.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                if(currentChapter > 0) SwitchChapter(currentChapter - 1);
            });
            btnNext.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                if(currentChapter < levelChapters.Count - 1)SwitchChapter(currentChapter + 1);
            });

            SwitchChapter(0);
        }
        private void SwitchChapter(int chapter)
        {
            currentChapter = chapter;
            
            chapterLabel.text = $"第{chapter + 1}章";
            
            btnPrev.SetEnabled(chapter > 0);
            btnNext.SetEnabled(chapter < levelChapters.Count - 1);
            
            levelDots.Clear();
            for (int i = 0; i < levelChapters.Count; i++)
            {
                VisualElement dot = new VisualElement();
                dot.AddToClassList("chapter-dot");
                if (i == chapter) dot.AddToClassList("chapter-dot--active");
                levelDots.Add(dot);
            }
        }
        
        private void BuildNavBar()
        {
            VisualElement navBar = mainRoot.Q<VisualElement>("nav-bar");
            if (navBar == null) return;

            navBar.Clear();

            foreach (NavButtonData buttonData in navButtons)
            {
                VisualElement btn = new ();
                btn.AddToClassList("nav-btn");
            
                VisualElement icon = new VisualElement();
                icon.AddToClassList("nav-icon");
                if (buttonData.icon != null) icon.style.backgroundImage = new StyleBackground(buttonData.icon);
                btn.Add(icon);
            
                if (buttonData.badge != 0)
                {
                    Label badge = new Label(buttonData.badge == -1 ? "!" : buttonData.badge.ToString());
                    badge.AddToClassList("nav-badge");
                    btn.Add(badge);
                }
            
                Label label = new Label(buttonData.label);
                label.AddToClassList("nav-label");

                btn.Add(label);
            
                btn.RegisterCallback<ClickEvent>(_ => OnNavClick(label.text));

                navBar.Add(btn);
            }
        }
        
        private void BuildEquipmentPopup()
        {
            equipmentPopup.style.display = DisplayStyle.None;
            
            equipmentPopup.RegisterCallback<ClickEvent>(_ => HideEquipmentPopup());
            
            VisualElement popupRoot = equipmentPopup.Q<VisualElement>("popup-root");
            popupRoot?.RegisterCallback<ClickEvent>(e => e.StopPropagation());
            
            Button cloBtn = equipmentPopup.Q<Button>("btn-close");
            cloBtn?.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                HideEquipmentPopup();
            });
            
            Button tabEquip   = equipmentPopup.Q<Button>("tab-equip");
            Button tabEnhance = equipmentPopup.Q<Button>("tab-enhance");
            Button tabSkill   = equipmentPopup.Q<Button>("tab-skill");

            tabEquip?.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                SwitchEquipTab("equip", tabEquip, tabEnhance, tabSkill);
            });
            tabEnhance?.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                SwitchEquipTab("enhance", tabEquip, tabEnhance, tabSkill);
            });
            tabSkill?.RegisterCallback<ClickEvent>(e =>
            {
                e.StopPropagation();
                SwitchEquipTab("skill", tabEquip, tabEnhance, tabSkill);
            });
        }

        private void SwitchEquipTab(string tab, params Button[] tabs)
        {
            foreach (Button t in tabs) t.RemoveFromClassList("tab-btn--active");
            
            switch (tab)
            {
                case "equip":   tabs[0].AddToClassList("tab-btn--active"); break;
                case "enhance": tabs[1].AddToClassList("tab-btn--active"); break;
                case "skill":   tabs[2].AddToClassList("tab-btn--active"); break;
            }
        }

        private Label selectedRoleNumsHint;
        private void BuildHeroes(RoleRegistry.RoleEntry[] heroes)
        {
            roleSelectPopup.style.display = DisplayStyle.None;
            roleSelectPopup.RegisterCallback<ClickEvent>(e => roleSelectPopup.style.display = DisplayStyle.None );
            
            VisualElement bgSelected = mainRoot.Q<VisualElement>("bg-selected");
            bgSelected.pickingMode = PickingMode.Ignore;
            bgSelected.RegisterCallback<ClickEvent>(e => e.StopPropagation());
            
            Button cancelBtn = mainRoot.Q<Button>("bt-cancel");
            cancelBtn.RegisterCallback<ClickEvent>(_ => roleSelectPopup.style.display = DisplayStyle.None);
            Button ensureBtn = mainRoot.Q<Button>("bt-ensure");
            ensureBtn.RegisterCallback<ClickEvent>(_ => EnterLevel(levelChapters[currentChapter], levelChapters[currentChapter].levelData[currentLevel]));
            
            ScrollView scrollView = mainRoot.Q<ScrollView>("hero-scroll");
            
            VisualElement track = mainRoot.Q<VisualElement>("hero-track");
            track.pickingMode = PickingMode.Ignore;
            track.Clear();
            Dictionary<VisualElement, RoleRegistry.RoleEntry> cardHeroMap = new ();
            
            Vector2 pointerDownPos = Vector2.zero;
            const float dragThreshold = 1f;
            scrollView.RegisterCallback<PointerDownEvent>(e =>
            {
                pointerDownPos = e.position;
            }, TrickleDown.TrickleDown);   

            scrollView.RegisterCallback<PointerUpEvent>(e =>
            {
                if (Vector2.Distance(e.position, pointerDownPos) > dragThreshold)
                    return;
                
                VisualElement target = e.target as VisualElement;
                while (target != null && !target.ClassListContains("hero-card"))
                    target = target.parent;

                if (target != null && cardHeroMap.TryGetValue(target, out RoleRegistry.RoleEntry hero))
                {
                    OnHeroSelected(hero, target);
                }
            }, TrickleDown.TrickleDown);
            
            foreach (RoleRegistry.RoleEntry hero in heroes)
            {
                VisualElement heroCard = new VisualElement();
                heroCard.AddToClassList("hero-card");
                heroCard.style.backgroundImage = new StyleBackground(hero.template.unSelectedIcon);

                Label nameLabel = new Label($"{hero.template.roleName} Lv.{hero.template.roleLevel}");
                nameLabel.AddToClassList("hero-card-name");
                nameLabel.pickingMode = PickingMode.Ignore;
                heroCard.Add(nameLabel);

                cardHeroMap[heroCard] = hero;

                track.Add(heroCard);
            }
            
            selectedRoleNumsHint = mainRoot.Q<Label>("lab-hintRoleNum");

#if UNITY_EDITOR
            Vector2 editorScrollStart = Vector2.zero;
            Vector2 editorOffsetStart = Vector2.zero;
            scrollView.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button != 0) return;
                editorScrollStart = e.mousePosition;
                editorOffsetStart = scrollView.scrollOffset;
            });
            scrollView.RegisterCallback<MouseMoveEvent>(e =>
            {
                if ((e.pressedButtons & 1) == 0) return;
                float deltaX = editorScrollStart.x - e.mousePosition.x;
                scrollView.scrollOffset = new Vector2(editorOffsetStart.x + deltaX, editorOffsetStart.y);
            });
#endif
        }
        
        private const int MaxSelectedRoleNums = 2;
        private readonly List<RoleRegistry.RoleEntry> selectedHeroes = new (MaxSelectedRoleNums);
        private void OnHeroSelected(RoleRegistry.RoleEntry hero, VisualElement heroCard)
        {
            bool isUnSelected = hero.template.selectedIcon != heroCard.style.backgroundImage.value.sprite;
            
            if (isUnSelected)
            {
                if(MaxSelectedRoleNums <= selectedHeroes.Count)
                {
                    Debug.Log("上场英雄已满！");
                    return;
                }
                
                selectedHeroes.Add(hero);
                heroCard.style.backgroundImage = new StyleBackground(hero.template.selectedIcon);
            }
            else
            {
                heroCard.style.backgroundImage = new StyleBackground(hero.template.unSelectedIcon);
                selectedHeroes.Remove(hero);
            }
            
            selectedRoleNumsHint.text = $"人数限制:{selectedHeroes.Count}/{MaxSelectedRoleNums}";
        }
        private void ShowEquipmentPopup()
        {
            equipmentPopup.style.display = DisplayStyle.Flex;
        }

        private void HideEquipmentPopup()
        {
            equipmentPopup.style.display = DisplayStyle.None;
        }
        
        private T SafeQuery<T>(string elementName) where T : VisualElement
        {
            T element = mainRoot.Q<T>(elementName);
            if (element == null) Debug.LogError($"[MainMenu.uxml] 找不到元素: name='{elementName}'");
            return element;
        }
        
        private void HideLevelPopup()
        {
            levelPopup.style.display = DisplayStyle.None;
        }
        void ShowLevelPopup()
        {
            levelPopup.style.display = DisplayStyle.Flex;
        }
        void SetRoleSelectPopup(DisplayStyle style)
        {
            roleSelectPopup.style.display = style;
        }
        private void EnterLevel(ChapterData chapter, LevelData level)
        {
            if (selectedHeroes.Count > 0)
            {
                levelContext.selectedHeroes = selectedHeroes;
                SceneLoader.Instance.LoadLevel(chapter, level);
            }
            else
            {
                Debug.Log("至少选择一个上场的英雄！");
            }
        }
        
        void OnEnterMode(GameModeData d)
        {
            Debug.Log($"进入玩法: {d.modeName}");
        }
        void OnNavClick(string label)
        { 
            if (label == "装备") ShowEquipmentPopup();
        }
    }
}
