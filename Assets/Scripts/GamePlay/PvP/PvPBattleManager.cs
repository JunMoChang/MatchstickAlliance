using System;
using AssetLoad;
using Fusion;
using GamePlay.GameModel.Level;
using GamePlay.Role.RoleData;
using UnityEngine;

namespace GamePlay.PvP
{
    public class PvPBattleManager : NetworkBehaviour
    {
        [Header("角色生成点")]
        [SerializeField] private Transform spawnPointA;
        [SerializeField] private Transform spawnPointB;

        [Header("回合配置")]
        [SerializeField] private float roundDuration = 60f;
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private float roundEndDelay = 3f;
        [SerializeField] private int winsNeeded = 2;
        
        
        [Networked] private NetworkBool PlayerAReady { get; set; }
        [Networked] private NetworkBool PlayerBReady { get; set; }
        [Networked] private byte _BattleState { get; set; }

        [Networked] private NetworkString<_32> PlayerARole { get; set; }
        [Networked] private NetworkString<_32> PlayerBRole { get; set; }

        [Networked] public int RoundNumber { get; private set; }
        [Networked] public int ScoreA { get; private set; }
        [Networked] public int ScoreB { get; private set; }
        [Networked] public float RoundTimer { get; private set; }
        [Networked] public byte CurrentRoundSubState { get; private set; }
        /// <summary> 比赛胜者 </summary>
        [Networked] public RoundResult MatchWinner { get; private set; }
        
        [Networked] public NetworkObject NetworkedCharacterA { get; private set; }
        [Networked] public NetworkObject NetworkedCharacterB {get; private set;}

        private PlayerRef playerA;
        private PlayerRef playerB;
        private bool rolesReported;
        
        public BattleState CurrentState => (BattleState)_BattleState;
        public PvPRoleSetup CharacterA => NetworkedCharacterA != null ? NetworkedCharacterA.GetComponent<PvPRoleSetup>() : null;
        public PvPRoleSetup CharacterB => NetworkedCharacterB != null ? NetworkedCharacterB.GetComponent<PvPRoleSetup>() : null;
        
        
        /// <summary> int - 玩家分数 </summary>
        public event Action<int, int> OnScoreChanged;
        public event Action<float> OnTimerChanged;
        /// <summary> int - 回合数 </summary>
        public event Action<int, RoundResult> OnRoundEnded;
        public event Action<RoundResult> OnMatchEnded;
        public event Action OnCountdownStarted;

        public event Action OnSpawned;
        private bool roundEndProcessed;
        
        public override void Spawned()
        {
            Debug.Log("[PvPBattleManager] Spawned. Waiting for players...");
            
            if (Object.HasStateAuthority)
            {
                _BattleState = (byte)BattleState.Waiting;
            }

            ReportMyRole();
            RPC_PlayerReady(Runner.LocalPlayer);
            
            OnSpawned?.Invoke();
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            switch (CurrentState)
            {
                case BattleState.Waiting:
                    CheckPlayersReady();
                    break;
                case BattleState.Loading:
                    break;
                case BattleState.Fighting:
                    UpdateRound();
                    break;
            }
        }
        
        private void ReportMyRole()
        {
            string roleStr = LevelContext.PvPSelectedRole?.roleName.ToString();
            if (string.IsNullOrEmpty(roleStr))
            {
                Debug.LogWarning("[PvPBattleManager] No role selected in LevelContext. Using default.");
                return;
            } 
            RPC_ReportRole(Runner.LocalPlayer, roleStr);
        }
        
        private void CheckPlayersReady()
        {
            if (!PlayerAReady || !PlayerBReady) return;
            if (!rolesReported) return;

            _BattleState = (byte)BattleState.Loading;
            Debug.Log("[PvPBattleManager] Both players ready with roles. Spawning characters...");
            SpawnCharacters();
        }

        private void SpawnCharacters()
        {
            if (Runner == null) return;

            string roleAName = PlayerARole.ToString();
            string roleBName = PlayerBRole.ToString();
            if (string.IsNullOrEmpty(roleAName) || string.IsNullOrEmpty(roleBName))
            {
                Debug.LogError("[PvPBattleManager] Missing role names, cannot spawn.");
                return;
            }

            if (!Enum.TryParse(roleAName, out RoleName nameA) || !Enum.TryParse(roleBName, out RoleName nameB))
            {
                Debug.LogError($"[PvPBattleManager] Invalid role names: {roleAName}, {roleBName}");
                return;
            }

            RoleRegistry.RoleEntry? entryA = GameDataManager.RoleRegistry?.GetRoleEntry(nameA);
            RoleRegistry.RoleEntry? entryB = GameDataManager.RoleRegistry?.GetRoleEntry(nameB);
            if (entryA == null || entryB == null)
            {
                Debug.LogError($"[PvPBattleManager] Role not found in registry: {roleAName} or {roleBName}");
                return;
            }

            try
            {
                NetworkedCharacterA = Runner.Spawn(
                    entryA.Value.prefab,
                    spawnPointA.position,
                    Quaternion.identity,
                    playerA,
                    (_, obj) => obj.GetComponent<PvPRoleSetup>().SyncedRoleName = roleAName
                );

                NetworkedCharacterB = Runner.Spawn(
                    entryB.Value.prefab,
                    spawnPointB.position,
                    Quaternion.identity,
                    playerB,
                    (_, obj) => obj.GetComponent<PvPRoleSetup>().SyncedRoleName = roleBName
                );
                
                _BattleState = (byte)BattleState.Fighting;
                
                StartNewRound();

                Debug.Log($"[PvPBattleManager] Characters spawned: {roleAName} vs {roleBName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PvPBattleManager] Failed to spawn characters: {e}");
            }
        }
        
        private void StartNewRound()
        {
            RoundNumber++;
            CurrentRoundSubState = (byte)RoundSubState.Countdown;
            RoundTimer = countdownDuration;
            roundEndProcessed = false;

            // 重置角色
            CharacterA?.Respawn(spawnPointA.position);
            CharacterB?.Respawn(spawnPointB.position);

            OnCountdownStarted?.Invoke();
            Debug.Log($"[PvPBattleManager] Round {RoundNumber} starting! Countdown...");
        }

        private void UpdateRound()
        {
            RoundSubState subState = (RoundSubState)CurrentRoundSubState;

            switch (subState)
            {
                case RoundSubState.Countdown:
                    UpdateStartCountdown();
                    break;
                case RoundSubState.Active:
                    UpdateActiveRound();
                    break;
                case RoundSubState.RoundEnd:
                    UpdateRoundEnd();
                    break;
                case RoundSubState.MatchEnd:
                    //等待 UI 处理
                    break;
            }
        }

        private void UpdateStartCountdown()
        {
            RoundTimer -= Runner.DeltaTime;
            OnTimerChanged?.Invoke(RoundTimer);

            if (RoundTimer <= 0f)
            {
                RoundTimer = roundDuration;
                CurrentRoundSubState = (byte)RoundSubState.Active;
                OnTimerChanged?.Invoke(RoundTimer);
                Debug.Log($"[PvPBattleManager] Round {RoundNumber} — FIGHT!");
            }
        }

        private void UpdateActiveRound()
        {
            RoundTimer -= Runner.DeltaTime;
            OnTimerChanged?.Invoke(RoundTimer);
            
            if (CharacterA != null && CharacterA.IsDead && !roundEndProcessed)
            {
                EndRound(RoundResult.Player2Win);
                return;
            }
            if (CharacterB != null && CharacterB.IsDead && !roundEndProcessed)
            {
                EndRound(RoundResult.Player1Win);
                return;
            }
            
            if (RoundTimer <= 0f)
            {
                RoundTimer = 0f;
                OnTimerChanged?.Invoke(0f);
                EndRoundByTimeout();
            }
        }
        
        private void EndRoundByTimeout()
        {
            if (roundEndProcessed) return;
            
            PvPRoleSetup roleA = CharacterA;
            PvPRoleSetup roleB = CharacterB;
            // 比较剩余 HP 百分比，高者胜（RoleContext 挂在子物体 VisualTransform 上，需向下查找）
            float hpPercentA = roleA != null && !roleA.IsDead
                ? roleA.CurrentHp / (roleA.GetComponentInChildren<RoleContext>()?.RuntimeData?.maxHealth ?? 1f) : 0f;
            float hpPercentB = roleB != null && !roleB.IsDead
                ? roleB.CurrentHp / (roleB.GetComponentInChildren<RoleContext>()?.RuntimeData?.maxHealth ?? 1f) : 0f;

            RoundResult winner = RoundResult.None;
            if (!Mathf.Approximately(hpPercentA, hpPercentB))
            {
                winner = hpPercentA < hpPercentB ? RoundResult.Player2Win : RoundResult.Player1Win;
            }
            Debug.Log($"[PvPBattleManager] Round {RoundNumber} timeout — HP% A:{hpPercentA:F2} B:{hpPercentB:F2} → Round Result:{winner}");
            EndRound(winner);
        }

        private void EndRound(RoundResult winner)
        {
            roundEndProcessed = true;

            if (winner == RoundResult.Player1Win) ScoreA++;
            else if(winner == RoundResult.Player2Win) ScoreB++;

            OnRoundEnded?.Invoke(RoundNumber, winner);
            OnScoreChanged?.Invoke(ScoreA, ScoreB);

            Debug.Log($"[PvPBattleManager] Round {RoundNumber} ended. Winner: Player {winner}. Score: {ScoreA}-{ScoreB}");

            // 检查是否比赛结束：先到 winsNeeded 分，或打满最大局数（winsNeeded*2-1）
            RoundResult matchWinner = RoundResult.None;
            if (ScoreA >= winsNeeded || ScoreB >= winsNeeded || RoundNumber >= winsNeeded * 2 - 1)
            {
                if(ScoreA >= winsNeeded)
                    matchWinner = RoundResult.Player1Win;
                else if (ScoreB >= winsNeeded)
                    matchWinner = RoundResult.Player2Win;

                CurrentRoundSubState = (byte)RoundSubState.MatchEnd;
                MatchWinner = matchWinner;
                OnMatchEnded?.Invoke(matchWinner);
                Debug.Log($"[PvPBattleManager] MATCH ENDED! Player {matchWinner} wins ({ScoreA}-{ScoreB})");
            }
            else
            {
                CurrentRoundSubState = (byte)RoundSubState.RoundEnd;
                RoundTimer = roundEndDelay;
            }
        }

        private void UpdateRoundEnd()
        {
            RoundTimer -= Runner.DeltaTime;
            OnTimerChanged?.Invoke(RoundTimer);

            if (RoundTimer <= 0f)
            {
                StartNewRound();
            }
        }
        
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_PlayerReady(PlayerRef player)
        {
            if (!Object.HasStateAuthority) return;

            if (playerA == PlayerRef.None || playerA == player)
            {
                playerA = player;
                PlayerAReady = true;
            }
            else if (playerB == PlayerRef.None || playerB == player)
            {
                playerB = player;
                PlayerBReady = true;
            }

            Debug.Log($"[PvPBattleManager] Player {player.PlayerId} is ready. A={PlayerAReady}, B={PlayerBReady}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RPC_ReportRole(PlayerRef player, string roleName)
        {
            if (!Object.HasStateAuthority) return;                               

            if (playerA == PlayerRef.None || playerA == player)
            {
                playerA = player;
                PlayerARole = roleName;
            }
            else if (playerB == PlayerRef.None || playerB == player)
            {
                playerB = player;
                PlayerBRole = roleName;
            }

            if (!string.IsNullOrEmpty(PlayerARole.ToString()) && !string.IsNullOrEmpty(PlayerBRole.ToString()))
            {
                rolesReported = true;
            }

            Debug.Log($"[PvPBattleManager] Player {player.PlayerId} selected role: {roleName}. Roles reported: {rolesReported}");
        }
    }
}
