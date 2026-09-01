using System.Collections;
using GamePlay.Role.RoleData;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GamePlay.PvP
{
    public class PvPHudView : MonoBehaviour
    {
        [Header("面板")]
        [SerializeField] private GameObject hudRoot;

        [Header("玩家 A")]
        [SerializeField] private TextMeshProUGUI playerANameText;
        [SerializeField] private Slider playerAhpBar;
        [SerializeField] private Image playerAhpFill;
        
        [Header("玩家 B")]
        [SerializeField] private TextMeshProUGUI playerBNameText;
        [SerializeField] private Slider playerBhpBar;
        [SerializeField] private Image playerBhpFill;

        [Header("计时 & 比分")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI roundText;
        [SerializeField] private GameObject overlay;
        private TextMeshProUGUI overlayText;

        [Header("颜色")]
        [SerializeField] private Color hpColorHigh = Color.green;
        [SerializeField] private Color hpColorMid = Color.yellow;
        [SerializeField] private Color hpColorLow = Color.red;
        [SerializeField] private float hpMidThreshold = 0.5f;
        [SerializeField] private float hpLowThreshold = 0.25f;

        [Header("引用")]
        [SerializeField] private PvPBattleManager battleManager;
        [SerializeField] private PvPRoundManager roundManager;

        private float maxHpA;
        private float maxHpB;
        private bool hudInitialized;
        private bool battleManagerSpawned;
        private void Awake()
        {
            if (battleManager == null) battleManager = FindAnyObjectByType<PvPBattleManager>();
            if (roundManager == null) roundManager = GetComponent<PvPRoundManager>();
            if (battleManager != null) battleManager.OnSpawned += () => battleManagerSpawned = true;
        }

        private void Start()
        {
            if (hudRoot != null) hudRoot.SetActive(true);
            if (overlay != null)
            {
                overlay.gameObject.SetActive(false);
                if(overlayText == null) overlayText = overlay.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (roundManager != null)
            {
                roundManager.OnScoreChanged += UpdateScore;
                roundManager.OnTimerUpdate += UpdateTimerDisplay;
                roundManager.OnRoundNumberChanged += UpdateRoundNumber;
                roundManager.OnCountdownStarted += ShowCountdown;
                roundManager.OnRoundEnded += OnRoundEnd;
                roundManager.OnMatchEnded += OnMatchEnd;
            }

            // 初始显示
            UpdateScore(0, 0);
            UpdateRoundNumber(1);
            UpdateTimerDisplay(60f);
        }

        private void Update()
        {
            // Object 为 null：runner 已关闭 / 对象已 Despawn（返回大厅时），此时访问 networked 属性会抛异常
            if (battleManager == null || !battleManagerSpawned || battleManager.Object == null) return;
            
            if (!hudInitialized && battleManager.CharacterA != null && battleManager.CharacterB != null)
            {
                Initialize(battleManager.CharacterA, battleManager.CharacterB);
                hudInitialized = true;
            }

            // 每帧轮询双方 HP
            UpdateHpBars();
            UpdateCountdownOverlay();
        }

        /// <summary>
        /// 初始化玩家名字和最大 HP
        /// </summary>
        private void Initialize(PvPRoleSetup roleA, PvPRoleSetup roleB)
        {
            if (playerANameText != null)
            {
                string nameA = roleA != null ? roleA.SyncedRoleName.ToString() : "Player A";
                playerANameText.text = nameA;
            }
            if (playerBNameText != null)
            {
                string nameB = roleB != null ? roleB.SyncedRoleName.ToString() : "Player B";
                playerBNameText.text = nameB;
            }

            // 读取最大 HP（RoleContext 挂在子物体 VirsualTransform 上，需向下查找）
            if (roleA != null)
            {
                RoleContext ctx = roleA.GetComponentInChildren<RoleContext>();
                maxHpA = ctx?.RuntimeData?.maxHealth ?? 1f;
            }
            if (roleB != null)
            {
                RoleContext ctx = roleB.GetComponentInChildren<RoleContext>();
                maxHpB = ctx?.RuntimeData?.maxHealth ?? 1f;
            }
        }

        private void UpdateHpBars()
        {
            PvPRoleSetup roleA = battleManager.CharacterA;
            PvPRoleSetup roleB = battleManager.CharacterB;

            if (roleA != null)
            {
                float hp = Mathf.Max(0f, roleA.CurrentHp);
                if (maxHpA <= 0f) maxHpA = roleA.GetComponentInChildren<RoleContext>()?.RuntimeData?.maxHealth ?? 1f;
                UpdateSingleHpBar(playerAhpBar, playerAhpFill, hp, maxHpA);
            }

            if (roleB != null)
            {
                float hp = Mathf.Max(0f, roleB.CurrentHp);
                if (maxHpB <= 0f) maxHpB = roleB.GetComponentInChildren<RoleContext>()?.RuntimeData?.maxHealth ?? 1f;
                UpdateSingleHpBar(playerBhpBar, playerBhpFill, hp, maxHpB);
            }
        }

        private void UpdateSingleHpBar(Slider bar, Image fill, float currentHp, float maxHp)
        {
            if (bar == null) return;

            float percent = maxHp > 0f ? currentHp / maxHp : 0f;
            bar.value = percent;

            if (fill != null)
            {
                fill.color = percent > hpMidThreshold ? hpColorHigh : percent > hpLowThreshold ? hpColorMid : hpColorLow;
            }
        }

        private void UpdateTimerDisplay(float time)
        {
            int seconds = Mathf.CeilToInt(time);
            timerText.text = $"{seconds}s";
            timerText.color = time <= 10f ? Color.red : Color.white;
        }

        private void UpdateScore(int scoreA, int scoreB)
        {
            scoreText.text = $"{scoreA} : {scoreB}";
        }

        private void UpdateRoundNumber(int roundNumber)
        {
            roundText.text = $"第 {roundNumber} 回合";
        }

        private void UpdateCountdownOverlay()
        {
            bool isCountdown = battleManager.CurrentRoundSubState == (byte)RoundSubState.Countdown;
            if (overlay != null) overlay.gameObject.SetActive(isCountdown);
            if (isCountdown && overlayText != null)
            {
                int secondsLeft = Mathf.CeilToInt(battleManager.RoundTimer);
                overlayText.text = secondsLeft.ToString();
            }
        }
        private void ShowCountdown()
        {
            overlay.gameObject.SetActive(true);
        }

        private IEnumerator HideStatusTextAfterDelay(float delay)
        {
            overlayText.gameObject.SetActive(true);
            float currentTimer = battleManager.RoundTimer;
            
            //countdownOverlayText.text = 
            yield return new WaitForSeconds(delay);
            
            overlayText.gameObject.SetActive(false);
        }
        
        
        private void OnRoundEnd(int roundNumber, RoundResult winner)
        {
            if (overlayText != null)
            {
                overlayText.text = $"Player {winner} Wins!";
                overlayText.gameObject.SetActive(true);
            }
        }

        private void OnMatchEnd(RoundResult winner)
        {
            if (overlayText != null)
            {
                overlayText.text = $"Player {winner} Wins the Match!";
                overlayText.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (roundManager != null)
            {
                roundManager.OnScoreChanged -= UpdateScore;
                roundManager.OnTimerUpdate -= UpdateTimerDisplay;
                roundManager.OnRoundNumberChanged -= UpdateRoundNumber;
                roundManager.OnCountdownStarted -= ShowCountdown;
                roundManager.OnRoundEnded -= OnRoundEnd;
                roundManager.OnMatchEnded -= OnMatchEnd;
            }
        }
    }
}
