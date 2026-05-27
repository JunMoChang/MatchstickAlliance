using GamePlay.GameModel.Level;
using GamePlay.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class EndedGame : MonoBehaviour
    {
        [SerializeField] private GameObject endedPanel;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button againButton;
        [SerializeField] private Button nextButton;
        
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI diamondPerText;
        [SerializeField] private TextMeshProUGUI diamondFirstText;
        [SerializeField] private GameObject lootContent;
        
        private void Show()
        {
            ChapterData chapterData = LevelContext.CurrentChapter;
            LevelData levelData = LevelContext.CurrentLevel;
            
            expText.text = chapterData.GetExp(levelData.levelIndex).ToString();
            goldText.text = chapterData.GetGold(levelData.levelIndex).ToString();
            diamondPerText.text = chapterData.DiamondsPerLevel.ToString();
            diamondFirstText.text = chapterData.DiamondsFirstLevel.ToString();
            
            endedPanel.SetActive(true);
        }

        void OnEnable()
        {
            exitButton.onClick.AddListener(ExitButtonOnClick);
            againButton.onClick.AddListener(AgainButtonOnClick);
            nextButton.onClick.AddListener(NextButtonOnClick);
            LevelManager.OnRewardCompleted += Show;
        }

        void OnDisable()
        {
            exitButton.onClick.RemoveListener(ExitButtonOnClick);
            againButton.onClick.RemoveListener(AgainButtonOnClick);
            nextButton.onClick.RemoveListener(NextButtonOnClick);
            LevelManager.OnRewardCompleted -= Show;
        }

        private void ExitButtonOnClick()
        {
            SceneLoader.Instance.LoadMainMenu();
        }

        private void AgainButtonOnClick()
        {
            SceneLoader.Instance.LoadLevel(LevelContext.CurrentChapter, LevelContext.CurrentLevel);
        }

        private void NextButtonOnClick()
        {
            LevelData nextLevel = GetNextLevel(LevelContext.CurrentChapter, LevelContext.CurrentLevel);

            if (nextLevel != null)
                SceneLoader.Instance.LoadLevel(LevelContext.CurrentChapter, nextLevel);
            else
                SceneLoader.Instance.LoadMainMenu();
        }
        
        private LevelData GetNextLevel(ChapterData currentChapter, LevelData currentLevel)
        {
            int currentIndex = currentLevel.levelIndex + 1;

            return currentIndex < currentChapter.levelData.Length ? currentChapter.levelData[currentIndex] : null;
        }
        
    }
}