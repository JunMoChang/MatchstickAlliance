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
        
        [SerializeField] private LevelContext levelContext;
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
            pauseGo.SetActive(!pauseGo.activeInHierarchy);
        }
        
        private void ExitButtonOnClick()
        {
            SceneLoader.Instance.LoadMainMenu();
        }

        private void ResetButtonOnClick()
        {
            SceneLoader.Instance.LoadLevel(levelContext.currentChapter, levelContext.currentLevel);
        }
    }
}