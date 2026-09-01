using System;
using UnityEngine;

namespace GamePlay.PvP
{
    public class PvPRoundManager : MonoBehaviour
    {
        [SerializeField] private PvPBattleManager battleManager;
        
        public event Action<int> OnRoundNumberChanged;
        public event Action<int, int> OnScoreChanged;
        public event Action<float> OnTimerUpdate;
        public event Action<int, RoundResult> OnRoundEnded;
        public event Action<RoundResult> OnMatchEnded;
        public event Action OnCountdownStarted;
        
        private int cachedRoundNumber;
        private int cachedScoreA;
        private int cachedScoreB;
        private float cachedRoundTimer;
        private byte cachedSubState;
        private bool initialized;

        private void Awake()
        {
            if (battleManager == null) TryGetComponent(out battleManager);
            
            if (battleManager == null)
            {
                Debug.LogError("[PvPRoundManager] PvPBattleManager reference is null!");
                return;
            }
            
            battleManager.OnSpawned += Initialize;
        }

        private void Initialize()
        {
            battleManager.OnScoreChanged += HandleScoreChanged;
            battleManager.OnTimerChanged += HandleTimerChanged;
            battleManager.OnRoundEnded += HandleRoundEnded;
            battleManager.OnMatchEnded += HandleMatchEnded;
            battleManager.OnCountdownStarted += HandleCountdownStarted;
            
            cachedRoundNumber = battleManager.RoundNumber;
            cachedScoreA = battleManager.ScoreA;
            cachedScoreB = battleManager.ScoreB;
            cachedRoundTimer = battleManager.RoundTimer;
            cachedSubState = battleManager.CurrentRoundSubState;
            initialized = true;

            battleManager.OnSpawned -= Initialize;
        }

        private void Update()
        {
            if (!initialized || battleManager == null || battleManager.Object == null) return;
            if (battleManager.Object.HasStateAuthority) return;

            DetectChanges();
        }

        #region client detect

        private void DetectChanges()
        {
            // 回合号变化
            if (battleManager.RoundNumber != cachedRoundNumber)
            {
                cachedRoundNumber = battleManager.RoundNumber;
                OnRoundNumberChanged?.Invoke(cachedRoundNumber);
            }

            // 比分变化
            if (battleManager.ScoreA != cachedScoreA || battleManager.ScoreB != cachedScoreB)
            {
                cachedScoreA = battleManager.ScoreA;
                cachedScoreB = battleManager.ScoreB;
                OnScoreChanged?.Invoke(cachedScoreA, cachedScoreB);
            }

            // 计时器变化
            float currentTimer = battleManager.RoundTimer;
            if (Mathf.Abs(currentTimer - cachedRoundTimer) > 0.05f)
            {
                cachedRoundTimer = currentTimer;
                OnTimerUpdate?.Invoke(cachedRoundTimer);
            }

            // 子状态变化 → Countdown 启动 / 比赛结束
            // （OnMatchEnded 事件只在 host 触发，客户端靠这里补发）
            byte currentState = battleManager.CurrentRoundSubState;
            if (currentState != cachedSubState)
            {
                cachedSubState = currentState;
                if (currentState == (byte)RoundSubState.Countdown)
                {
                    OnCountdownStarted?.Invoke();
                }
                else if (currentState == (byte)RoundSubState.MatchEnd)
                {
                    OnMatchEnded?.Invoke(battleManager.MatchWinner);
                }
            }
        }

        #endregion


        #region host detect

        private void HandleScoreChanged(int scoreA, int scoreB)
        {
            cachedScoreA = scoreA;
            cachedScoreB = scoreB;
            OnScoreChanged?.Invoke(scoreA, scoreB);
        }

        private void HandleTimerChanged(float timer)
        {
            cachedRoundTimer = timer;
            OnTimerUpdate?.Invoke(timer);
        }

        private void HandleRoundEnded(int roundNumber, RoundResult winner)
        {
            cachedRoundNumber = roundNumber;
            cachedSubState = (byte) RoundSubState.RoundEnd;
            OnRoundEnded?.Invoke(roundNumber, winner);
        }

        private void HandleMatchEnded(RoundResult winner)
        {
            cachedSubState = (byte) RoundSubState.MatchEnd;
            OnMatchEnded?.Invoke(winner);
        }

        private void HandleCountdownStarted()
        {
            cachedSubState = (byte) RoundSubState.Countdown;

            int currentRound = battleManager.RoundNumber;
            if (currentRound != cachedRoundNumber)
            {
                cachedRoundNumber = currentRound;
                OnRoundNumberChanged?.Invoke(cachedRoundNumber);
            }
            OnCountdownStarted?.Invoke();
        }

        #endregion
        

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.OnSpawned -= Initialize;
                battleManager.OnScoreChanged -= HandleScoreChanged;
                battleManager.OnTimerChanged -= HandleTimerChanged;
                battleManager.OnRoundEnded -= HandleRoundEnded;
                battleManager.OnMatchEnded -= HandleMatchEnded;
                battleManager.OnCountdownStarted -= HandleCountdownStarted;
            }
        }
    }
}
