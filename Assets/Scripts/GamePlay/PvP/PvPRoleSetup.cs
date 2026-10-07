using System.Collections.Generic;
using AssetLoad;
using Fusion;
using GamePlay.GameModel;
using GamePlay.GameModel.Level;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using UnityEngine;

namespace GamePlay.PvP
{
    public class PvPRoleSetup : NetworkBehaviour
    {
        [Header("引用")]
        [SerializeField] private RoleContext roleContext;
        [SerializeField] private Animator animator;
        /// <summary> Fusion 自带插值组件（挂根物体，须关闭 Sync Scale）。启用时自研快照插值自动让位 </summary>
        [SerializeField] private NetworkTransform networkTransform;

        #region Networked

        [Networked] private Vector2 NetworkedPosition {get; set;}

        /// <summary> 网络 HP </summary>
        [Networked] private float NetworkedHp { get; set; }

        [Networked] private int RespawnTrigger { get; set; }

        /// <summary> 朝向 </summary>
        [Networked] private NetworkBool NetworkedFacingRight { get; set; }

        /// <summary> 攻击触发计数 </summary>
        [Networked] private int AttackTrigger { get; set; }

        /// <summary> 技能触发计数 </summary>
        [Networked] private int SkillTrigger { get; set; }

        /// <summary> 技能索引（proxy 端据此播放技能动画） </summary>
        [Networked] private int NetworkedSkillIndex { get; set; }

        /// <summary> 受击触发计数 </summary>
        [Networked] private int HitTrigger { get; set; }

        /// <summary> 死亡触发计数（与攻击/受击/技能一致，用计数表达一次性事件） </summary>
        [Networked] private int DeathTrigger { get; set; }
        
        /// <summary> 是否死亡 </summary>
        [Networked] private NetworkBool NetworkedIsDead { get; set; }

        /// <summary> 移动速度（动画用） </summary>
        [Networked] private float NetworkedSpeed { get; set; }

        /// <summary> 角色名 </summary>
        [Networked] public NetworkString<_32> SyncedRoleName { get; set; }
        [Networked] private bool PrevAttackInput { get; set; }

        #endregion
        
        public bool IsDead => NetworkedIsDead;
        public float CurrentHp => NetworkedHp;

        private ChangeDetector changeDetector;
        private float prevNetworkedHp;
        private IRoleStrategy strategy;
        private bool roleInitialized;
        private Vector3 spawnPosition;

        /// <summary> proxy 端渲染插值快照 </summary>
        private struct PositionSnapshot
        {
            /// <summary> 记录时的仿真时间  </summary>
            public float simulationTime;
            /// <summary> 记录时的渲染帧号 </summary>
            public int frame;
            /// <summary> 记录时的本地时间 </summary>
            public float localReceiveTime;
            
            public Vector2 position;
        }
  
        /// <summary> proxy 端位置插值缓冲 </summary>
        private readonly List<PositionSnapshot> positionSnapshots = new();
        private const int MaxSnapshots = 64;

        private void Awake()
        {
            if (roleContext == null) roleContext = GetComponentInChildren<RoleContext>();
            if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
            if (networkTransform == null) networkTransform = GetComponent<NetworkTransform>();
        }

        /// <summary> 位置插值是否交给 Fusion 的 NetworkTransform（组件存在且启用时） </summary>
        private bool UseFusionInterpolation => networkTransform != null && networkTransform.enabled;

        /// <summary>
        /// 瞬移。挂了 NetworkTransform 必须走 Teleport，否则插值会把角色从旧位置平滑拖过去
        /// </summary>
        private void TeleportTo(Vector3 position)
        {
            if (UseFusionInterpolation) networkTransform.Teleport(position);
            else transform.position = position;
        }

        public override void Spawned()
        {
            if (LevelContext.IsPvPMode)
            {
                SetLayerRecursively(transform, LayerMask.NameToLayer("Opponent"));
            }

            changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);

            InitializeRoleIfNeeded();

            Strategy?.SetPhysicsAuthority(Object.HasStateAuthority || Object.HasInputAuthority);
            
            if (Object.HasStateAuthority && roleContext?.RuntimeData != null)
            {
                NetworkedHp = roleContext.RuntimeData.maxHealth;
                NetworkedIsDead = false;
                NetworkedFacingRight = true;
            }
            
            if (!Object.HasStateAuthority)
            {
                // 出生时对齐位置：走 Teleport，避免 NetworkTransform 从 prefab 默认位置插值过来
                TeleportTo(NetworkedPosition);
                roleContext?.Flip(NetworkedFacingRight);
            }

            spawnPosition = transform.position;
            prevNetworkedHp = NetworkedHp;

                Debug.Log($"[PvPRoleSetup] Spawned. InputAuth={Object.HasInputAuthority}, StateAuth={Object.HasStateAuthority}, Role={SyncedRoleName}");
        }

        private void InitializeRoleIfNeeded()
        {
            if (roleInitialized) return;
            if (roleContext == null) return;
            if (roleContext.RuntimeData != null) { roleInitialized = true; return; }

            string roleNameStr = SyncedRoleName.ToString();
            if (string.IsNullOrEmpty(roleNameStr)) return;

            if (!System.Enum.TryParse(roleNameStr, out RoleName roleName))
            {
                Debug.LogWarning($"[PvPRoleSetup] Unknown role name: {roleNameStr}");
                return;
            }
            
            RoleRegistry.RoleEntry? entry = GameDataManager.RoleRegistry?.GetRoleEntry(roleName);
            if (entry == null)
            {
                Debug.LogError($"[PvPRoleSetup] Role not found in registry: {roleName}");
                return;
            }
            
            RoleSaveData saveData = new RoleSaveData { roleName = roleName, roleLevel = 1 };
            entry.Value.template.FirstLoadSaveData(saveData);
            roleContext.Initialize(entry.Value.template, saveData);

            strategy = RoleFactory.CreateRoleStrategy(roleName, roleContext);

            roleInitialized = true;
            Debug.Log($"[PvPRoleSetup] Role initialized: {roleName}");
        }

        private IRoleStrategy Strategy
        {
            get
            {
                strategy ??= roleContext?.Strategy;
                return strategy;
            }
        }

        public override void FixedUpdateNetwork()
        {
            float deltaTime = Runner.DeltaTime;

            if (Object.HasStateAuthority)
            {
                if (GetInput(out PvPInput input))
                {
                    Vector2 faceDir = Strategy?.Move(input.moveDirection) ?? Vector2.zero;
                 
                    if (faceDir.x != 0f) NetworkedFacingRight = faceDir.x > 0f;

                    bool attackPressed = input.attack && !PrevAttackInput;
                    if (attackPressed && !Runner.IsResimulation)
                    {
                        Strategy?.PerformAttack();
                        TriggerAttack();
                    }
                    PrevAttackInput = input.attack;
                    
                    if (input.skillTriggered && input.skillIndex > 0)
                    {
                        Strategy?.UseSkill(input.skillIndex - 1);
                        
                        NetworkedSkillIndex = input.skillIndex - 1;
                        SkillTrigger++;
                    }
                }
                
                NetworkedPosition = transform.position;
                NetworkedSpeed = animator.GetFloat(AnimationParameters.Speed);
                
                if (roleContext != null && roleContext.RuntimeData != null)
                {
                    float currentHp = roleContext.RuntimeData.currentHealth;
                    
                    if (currentHp < prevNetworkedHp) TriggerHit();
                    
                    NetworkedHp = currentHp;

                    bool isDead = currentHp <= 0f;
                    if (isDead && !NetworkedIsDead)
                    {
                        NetworkedIsDead = true;
                        DeathTrigger++;
                    }
                    prevNetworkedHp = NetworkedHp;
                }
            }
            else if (Object.HasInputAuthority)
            {
                if (GetInput(out PvPInput input))
                {
                    Strategy?.Move(input.moveDirection);

                    bool attackPressed = input.attack && !PrevAttackInput;
                    if (attackPressed && !Runner.IsResimulation) Strategy?.PerformAttack();
                    PrevAttackInput = input.attack;
                    
                    if (input.skillTriggered && input.skillIndex > 0) Strategy?.UseSkill(input.skillIndex - 1);
                }

                NetworkedPosition = transform.position;
            }
            
            if(Object.HasStateAuthority || Object.HasInputAuthority) Strategy?.FixedTick(deltaTime);
            
            if (!Object.HasStateAuthority && !Object.HasInputAuthority)
            {
                RecordSnapshot(Runner.SimulationTime, NetworkedPosition);
            }
        }

        /// <summary>
        /// 记录插值快照
        /// </summary>
        /// <param name="time">Runner.SimulationTime</param>
        /// <param name="position">NetworkedPosition</param>
        private void RecordSnapshot(float time, Vector2 position)
        {
            if (positionSnapshots.Count > 0 && time < positionSnapshots[^1].simulationTime)
                positionSnapshots.Clear();

            if (positionSnapshots.Count > 0 && positionSnapshots[^1].frame == Time.frameCount)
                positionSnapshots.RemoveAt(positionSnapshots.Count - 1);

            positionSnapshots.Add(new PositionSnapshot
            {
                simulationTime = time,
                frame = Time.frameCount,
                localReceiveTime = Time.unscaledTime,
                position = position
            });
            if (positionSnapshots.Count > MaxSnapshots) positionSnapshots.RemoveAt(0);
        }

        /// <summary>
        /// proxy 端位置插值。插值点落在历史区间（延迟约 1 tick）
        /// </summary>
        private void ApplyInterpolatedVisual()
        {
            if (positionSnapshots.Count == 0)
            {
                transform.position = NetworkedPosition;
                return;
            }

            // 插值点 = 当前时刻回退 1 tick，落在快照缓冲的历史区间内
            float targetTime = Time.unscaledTime - Runner.DeltaTime;

            // 线性查找 targetTime 所在区间 [i, i+1]
            int i = 0;
            while (i < positionSnapshots.Count - 1 && positionSnapshots[i + 1].localReceiveTime <= targetTime) i++;

            PositionSnapshot a = positionSnapshots[i];
            PositionSnapshot b = positionSnapshots[Mathf.Min(i + 1, positionSnapshots.Count - 1)];

            float t = Mathf.InverseLerp(a.localReceiveTime, b.localReceiveTime, targetTime);
            transform.position = Vector2.Lerp(a.position, b.position, t);
        }

        public override void Render()
        {
            if (!Object.HasStateAuthority && !Object.HasInputAuthority)
            {
                // 由 NetworkTransform 插值时不能再自研插值，否则两边同时写 transform.position
                if (!UseFusionInterpolation) ApplyInterpolatedVisual();
                animator.SetFloat(AnimationParameters.Speed, NetworkedSpeed);
            }
            if (!Object.HasStateAuthority && roleContext != null && roleContext.RuntimeData != null)
            {
                roleContext.RuntimeData.currentHealth = NetworkedHp;
            }
            Strategy?.Tick(Time.deltaTime);
            if (changeDetector == null) return;

            foreach (string change in changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(AttackTrigger):
                        if (!Object.HasStateAuthority && !Object.HasInputAuthority)
                            Strategy?.PerformAttack();
                        break;
                    case nameof(SkillTrigger):
                        if (!Object.HasStateAuthority && !Object.HasInputAuthority)
                            Strategy?.UseSkill(NetworkedSkillIndex);
                        break;
                    case nameof(HitTrigger):
                        if (Object.HasStateAuthority) break;
                        if (Strategy == null || !Strategy.IsUninterruptible) Strategy?.OnHit();
                        break;
                    case nameof(DeathTrigger):
                        OnDeathTriggered();
                        break;
                    case nameof(RespawnTrigger):
                        TeleportTo(spawnPosition);
                        positionSnapshots.Clear();
                        // 客户端本地模拟也要复位物理/动作状态
                        Strategy?.OnRespawn();
                        break;
                    case nameof(NetworkedFacingRight):
                        // 朝向翻转一次
                        if (!Object.HasStateAuthority && !Object.HasInputAuthority)
                            roleContext?.Flip(NetworkedFacingRight);
                        break;
                }
            }
        }

        private static void SetLayerRecursively(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t) SetLayerRecursively(child, layer);
        }

        
        private void TriggerAttack()
        {
            if (!Object.HasStateAuthority) return;
            AttackTrigger++;
        }
        
        private void TriggerHit()
        {
            if (!Object.HasStateAuthority) return;
            HitTrigger++;
        }
        
        private void OnDeathTriggered()
        {
            if (animator == null) return;
            
            animator.SetBool(AnimationParameters.Death_Bool, true);
            Debug.Log("[PvPRoleSetup] Death animation triggered.");
        }

        /// <summary>
        /// 回合间重置
        /// </summary>
        /// <param name="position">初始位置</param>
        public void Respawn(Vector3 position)
        {
            if (!Object.HasStateAuthority) return;

            if (roleContext?.RuntimeData != null)
            {
                roleContext.RuntimeData.currentHealth = roleContext.RuntimeData.maxHealth;
                NetworkedHp = roleContext.RuntimeData.maxHealth;
            }
            
            NetworkedIsDead = false;
            RespawnTrigger++;
            TeleportTo(position);
            NetworkedPosition = position;
            spawnPosition = position;
            
            Strategy?.OnRespawn();
            
            NetworkedFacingRight = true;
            roleContext?.Flip(true);

            if (animator != null)
            {
                animator.SetBool(AnimationParameters.Death_Bool, false);
                animator.ResetTrigger(AnimationParameters.Attacked_Trigger);
                animator.SetInteger(AnimationParameters.NormalAttack, -1);
                animator.SetFloat(AnimationParameters.Speed, 0f);
            }

            Debug.Log($"[PvPRoleSetup] Respawned at {position}");
        }
    }
}
