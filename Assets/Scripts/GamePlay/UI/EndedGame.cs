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
        
        [SerializeField] private LevelContext levelContext;

        public void Show()
        {
            endedPanel.SetActive(true);
        }

        void OnEnable()
        {
            exitButton.onClick.AddListener(ExitButtonOnClick);
            againButton.onClick.AddListener(AgainButtonOnClick);
            nextButton.onClick.AddListener(NextButtonOnClick);
        }

        void OnDisable()
        {
            exitButton.onClick.RemoveListener(ExitButtonOnClick);
            againButton.onClick.RemoveListener(AgainButtonOnClick);
            nextButton.onClick.RemoveListener(NextButtonOnClick);
        }

        private void ExitButtonOnClick()
        {
            SceneLoader.Instance.LoadMainMenu();
        }

        private void AgainButtonOnClick()
        {
            SceneLoader.Instance.LoadLevel(levelContext.currentChapter, levelContext.currentLevel);
        }

        private void NextButtonOnClick()
        {
            LevelData nextLevel = GetNextLevel(levelContext.currentChapter, levelContext.currentLevel);
                
            if (nextLevel != null) 
                SceneLoader.Instance.LoadLevel(levelContext.currentChapter, nextLevel);
            else 
                SceneLoader.Instance.LoadMainMenu();
        }
        
        private LevelData GetNextLevel(ChapterData currentChapter, LevelData currentLevel)
        {
            int currentIndex = currentLevel.levelIndex + 1;

            return currentIndex <= currentChapter.levelData.Length ? currentChapter.levelData[currentIndex] : null;
        }
        
    }
}