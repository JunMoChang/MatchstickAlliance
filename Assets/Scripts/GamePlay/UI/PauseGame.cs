using GamePlay.GameModel.Level;
using GamePlay.PlayerDataHandle;
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

            // PvP 模式不暂停游戏时间（会破坏 Fusion 网络同步），改为仅隐藏输入
            if (LevelContext.IsPvPMode)
            {
                // 禁用/启用输入
                var playerInput = FindAnyObjectByType<PlayerInputHandler>();
                if (playerInput != null) playerInput.enabled = !isPaused;
                return;
            }

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