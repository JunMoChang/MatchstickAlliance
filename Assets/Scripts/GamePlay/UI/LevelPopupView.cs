using System;
using System.Collections.Generic;
using GamePlay.GameModel.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class LevelPopupView : MonoBehaviour
    {
        public event Action<ChapterData, LevelData> OnLevelClicked;

        [SerializeField] GameObject rootPanel;
        [SerializeField] Button backgroundDismiss;
        [SerializeField] GameObject contentPanel;

        [SerializeField] TextMeshProUGUI chapterLabel;
        [SerializeField] Button[] levelButtons;
        [SerializeField] Button closeButton;
        [SerializeField] Button prevButton;
        [SerializeField] Button nextButton;
        [SerializeField] Transform dotsContainer;
        [SerializeField] private Sprite datIconSelected;
        [SerializeField] private Sprite datIconUnselected;
        [SerializeField] GameObject dotPrefab;

        private List<ChapterData> chaptersData;
        private int currentChapter;
        private readonly List<GameObject> dots = new();

        public void Show(List<ChapterData> chapterDataList, int startChapter = 1)
        {
            chaptersData = chapterDataList;
            rootPanel.SetActive(true);

            SwitchChapter(startChapter);
        }

        private void Hide()
        {
            rootPanel.SetActive(false);
        }

        void OnEnable()
        {
            if (backgroundDismiss) backgroundDismiss.onClick.AddListener(Hide);
            if (closeButton) closeButton.onClick.AddListener(Hide);
            if (prevButton) prevButton.onClick.AddListener(OnPrevChapter);
            if (nextButton) nextButton.onClick.AddListener(OnNextChapter);

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int index = i;
                if (levelButtons[i]) levelButtons[i].onClick.AddListener(() => OnLevelButtonClick(index));
            }
        }

        void OnDisable()
        {
            if (backgroundDismiss) backgroundDismiss.onClick.RemoveListener(Hide);
            if (closeButton) closeButton.onClick.RemoveListener(Hide);
            if (prevButton) prevButton.onClick.RemoveListener(OnPrevChapter);
            if (nextButton) nextButton.onClick.RemoveListener(OnNextChapter);

            foreach (Button btn in levelButtons)
            {
                if (btn) btn.onClick.RemoveAllListeners();
            }
        }

        private void OnPrevChapter()
        {
            if (chaptersData == null) return;
            
            SwitchChapter(Mathf.Max(1, currentChapter - 1));
        }

        private void OnNextChapter()
        {
            if (chaptersData == null) return;
            
            SwitchChapter(Mathf.Min(chaptersData.Count, currentChapter + 1));
        }

        private void OnLevelButtonClick(int levelButtonIndex)
        {
            if (chaptersData == null || currentChapter >= chaptersData.Count) return;
            
            ChapterData chapterData = chaptersData[currentChapter - 1];
            if (levelButtonIndex >= chapterData.levelData.Length) return;
            
            Debug.Log($"总章节数:{chaptersData.Count} 当前章节:{currentChapter} 当前关卡:{levelButtonIndex}");
            OnLevelClicked?.Invoke(chapterData, chapterData.levelData[levelButtonIndex]);
        }

        private void SwitchChapter(int chapter)
        {
            if (chaptersData == null || chapter < 1 || chapter > chaptersData.Count) return;

            currentChapter = chapter;
            ChapterData chapterData = chaptersData[chapter - 1];

            chapterLabel.text = $"第{chapter}章";
            
            if (prevButton) prevButton.interactable = chapter > 1;
            if (nextButton) nextButton.interactable = chapter < chaptersData.Count;
            
            RefreshDots();
            
            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i] == null) continue;

                bool hasLevel = i < chapterData.levelData.Length;
                levelButtons[i].gameObject.SetActive(hasLevel);
            }
        }

        private void RefreshDots()
        {
            foreach (GameObject dot in dots) Destroy(dot);
            dots.Clear();

            if (dotPrefab == null || dotsContainer == null || chaptersData == null) return;

            for (int i = 0; i < chaptersData.Count; i++)
            {
                GameObject dot = Instantiate(dotPrefab, dotsContainer);
                Image dotImage = dot.GetComponent<Image>();
                dotImage.sprite = i == currentChapter - 1 ? datIconSelected :  datIconUnselected;

                dots.Add(dot);
            }
        }
    }
}
