using GamePlay.GameModel.Level;
using GamePlay.Scene;
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

        public void Show()
        {
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
            SceneLoader.Instance.LoadLevel(LevelContext.Instance.currentChapter, LevelContext.Instance.currentLevel);
        }

        private void NextButtonOnClick()
        {
            LevelData nextLevel = GetNextLevel(LevelContext.Instance.currentChapter, LevelContext.Instance.currentLevel);

            if (nextLevel != null)
                SceneLoader.Instance.LoadLevel(LevelContext.Instance.currentChapter, nextLevel);
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