using GamePlay.GameModel.Level;
using GamePlay.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class PauseGame : MonoBehaviour
    {
        [SerializeField] private GameObject pauseGo;
        [SerializeField] private Button btPause;
        [SerializeField] private Button btExit;
        [SerializeField] private Button btReset;
        void OnEnable()
        {
            btPause.onClick.AddListener(PauseButtonOnClick);
            btExit.onClick.AddListener(ExitButtonOnClick);
            btReset.onClick.AddListener(ResetButtonOnClick);
        }

        void OnDisable()
        {
            btPause.onClick.RemoveListener(PauseButtonOnClick);
            btExit.onClick.RemoveListener(ExitButtonOnClick);
            btReset.onClick.RemoveListener(ResetButtonOnClick);
        }
        
        private void PauseButtonOnClick()
        {
            bool isPaused = !pauseGo.activeInHierarchy;
            pauseGo.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
        }

        private void ExitButtonOnClick()
        {
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadMainMenu();
        }

        private void ResetButtonOnClick()
        {
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadLevel(LevelContext.CurrentChapter, LevelContext.CurrentLevel);
        }
    }
}