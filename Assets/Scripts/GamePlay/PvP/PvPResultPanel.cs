using Fusion;
using GamePlay.GameModel.Level;
using GamePlay.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.PvP
{
    public class PvPResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject resultPanel;

        [Header("结果文本")]
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI resultDetailText;

        [Header("按钮")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button returnToLobbyButton;

        [Header("引用")]
        [SerializeField] private PvPBattleManager battleManager;
        [SerializeField] private PvPRoundManager roundManager;

        private void Awake()
        {
            if (battleManager == null) battleManager = FindAnyObjectByType<PvPBattleManager>();
            if (roundManager == null) roundManager = FindAnyObjectByType<PvPRoundManager>();

            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void OnEnable()
        {
            playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
            returnToLobbyButton?.onClick.AddListener(OnReturnToLobbyClicked);
            
            if (roundManager != null)
            {
                roundManager.OnMatchEnded += Show;
            }
            
            if (PvPNetworkManager.Instance != null)
            {
                PvPNetworkManager.Instance.OnReturnToLobbyRequested += ReturnToLobby;
            }
        }

        private void OnDisable()
        {
            playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
            returnToLobbyButton?.onClick.RemoveListener(OnReturnToLobbyClicked);

            if (roundManager != null)
            {
                roundManager.OnMatchEnded -= Show;
            }

            if (PvPNetworkManager.Instance != null)
            {
                PvPNetworkManager.Instance.OnReturnToLobbyRequested -= ReturnToLobby;
            }
        }

        /// <summary>
        /// 显示结算面板
        /// </summary>
        private void Show(RoundResult winner)
        {
            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;
            bool localPlayerWon = (isHost && winner == RoundResult.Player1Win) || (!isHost && winner == RoundResult.Player2Win);

            if (resultTitleText != null)
            {
                if (localPlayerWon)
                {
                    resultTitleText.text = "胜利！";
                    resultTitleText.color = Color.yellow;
                }
                else if (winner == RoundResult.None)
                {
                    resultTitleText.text = "平局";
                    resultTitleText.color = Color.gray;
                }
                else
                {
                    resultTitleText.text = "失败";
                    resultTitleText.color = Color.green;
                }
            }

            if (resultDetailText != null && battleManager != null)
            {
                resultDetailText.text = $"{battleManager.ScoreA} : {battleManager.ScoreB}";
            }

            resultPanel.SetActive(true);
            Debug.Log($"[PvPResultPanel] Match ended. Local player {(localPlayerWon ? "won" : "lost")}");
        }

        private void OnPlayAgainClicked()
        {
            NetworkRunner runner = PvPNetworkManager.Instance?.Runner;
            if (runner != null && runner.IsRunning)
            {
                Debug.Log("[PvPResultPanel] Loading PvP scene again via Fusion...");
                runner.LoadScene(PvPNetworkManager.Instance.PvpSceneRef);
            }
            else
            {
                Debug.LogWarning("[PvPResultPanel] Runner not running, loading via SceneManager fallback.");
                SceneLoader.Instance.LoadScene("PVPBattleScene");
            }
        }

        private void OnReturnToLobbyClicked()
        {
            Debug.Log("[PvPResultPanel] Returning to lobby...");
            
            ReturnToLobby();
        }

        /// <summary>
        /// 返回大厅
        /// </summary>
        private void ReturnToLobby()
        {
            if (LevelContext.IsReturningToLobby) return;
            
            LevelContext.IsReturningToLobby = true;

            LevelContext.IsPvPMode = false;
            LevelContext.ReturnToPvPLobby = true;

            PvPNetworkManager.Instance?.Shutdown();

            SceneLoader.Instance?.LoadMainMenu();
        }
    }
}
