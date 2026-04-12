using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.Scene;
using UI;
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
        
        [Header("导航按钮")]
        [SerializeField] List<NavButtonData> navButtons = new();
        
        private VisualElement mainRoot;
        private VisualElement cardTrack;
        private VisualElement levelPopup;
        private VisualElement currentActive;
        
        private VisualElement levelBg;
        private VisualElement levelContent;
        private VisualElement levelDots;
        private Label chapterLabel;
        private Button closeBtn;
        private Button btnPrev;
        private Button btnNext;
        
        private int currentChapter;
        
        private void OnEnable()
        {
            mainRoot = uiDocument.rootVisualElement;
        
            cardTrack = SafeQuery<VisualElement>("card-track");
            levelPopup = SafeQuery<VisualElement>("level-popup");
        
            if (backgroundSprite != null)
            {
                VisualElement bg = mainRoot.Q<VisualElement>("background");
                if (bg != null) bg.style.backgroundImage = new StyleBackground(backgroundSprite);
            }
            
            BuildCards();
            BuildNavBar();
            BuildLevel();
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
            
            levelContent.Clear();
            foreach (LevelData level in levelChapters[chapter].levelData)
            {
                int index = 0;
                VisualElement icon = new VisualElement();
                icon.AddToClassList("level-icon");
                icon.style.backgroundImage = new StyleBackground(level.sprite.levelSprite);
                icon.RegisterCallback<ClickEvent>(_ => OnLevelSelected(levelChapters[chapter], levelChapters[chapter].levelData[index++]));
                levelContent.Add(icon);
            }
            
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
        
        private void OnLevelSelected(ChapterData chapter, LevelData level)
        {
            SceneLoader.Instance.LoadLevel(chapter, level);
        }
        
        void OnEnterMode(GameModeData d)
        {
            Debug.Log($"进入玩法: {d.modeName}");
        }
        void OnNavClick(string label)
        { 
            Debug.Log($"导航点击: {label}");
        }
    }
}