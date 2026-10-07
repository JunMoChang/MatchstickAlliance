using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamePlay.GameModel.Level;
using GamePlay.PvP;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.PlayerDataHandle
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private PlayerInputHandler playerInputHandler;
        /// <summary> 单个角色资源加载的最长等待（秒），超过则放弃本次初始化并记录错误，需小于 SceneLoader 的 readyTimeout </summary>
        [SerializeField, Min(1f)] private float roleLoadTimeout = 6f;
        private readonly List<GameObject> roleInstances = new ();
        /// <summary> 已创建但尚未启用的角色策略；由 InitSelectedRolesRoutine 按本关角色数分配，Cleanup 置 null 表示"本关没有可用角色" </summary>
        private IRoleStrategy[] pendingStrategies;
        /// <summary> 预热缓存：角色 → 已发起的加载任务。同一个 AssetReference 不能重复 LoadAssetAsync，缓存让预热与准备阶段复用同一次加载 </summary>
        private readonly Dictionary<RoleRegistry.RoleEntry, Task<GameObject>> preloadTasks = new (3);

        private static bool exists;
        /// <summary> 重复实例标记：Awake 里已销毁自己，Start 里不能再订阅，否则会留下永远不会上报的幽灵订阅者 </summary>
        private bool isDuplicate;
        private bool isLoadedSuccess;

        private void Awake()
        {
            if (!LevelContext.IsPvPMode)
            {
                if (exists)
                {
                    isDuplicate = true;
                    Destroy(gameObject);
                    return;
                }
                exists = true;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (isDuplicate) return;

            playerInputHandler.enabled = false;
            SceneLoader.OnPrepareLevel += InitSelectedRoles;
            SceneLoader.OnLevelLoaded += SceneLoaded;
            SceneLoader.OnLevelExit += Cleanup;
        }

        /// <summary>
        /// PvP 模式准备：在 Runner.LoadScene 之前由 PvPLobbyPanel 调用，
        /// 订阅 Fusion 的 OnSceneLoadDone，当所有端场景就绪后启用输入。
        /// </summary>
        public void PrepareForPvP()
        {
            if (PvPNetworkManager.Instance != null)
            {
                PvPNetworkManager.Instance.OnPvPSceneLoaded -= OnPvPSceneLoaded;
                PvPNetworkManager.Instance.OnPvPSceneLoaded += OnPvPSceneLoaded; 
            }
        }

        private void OnPvPSceneLoaded()
        {
            if (PvPNetworkManager.Instance != null) PvPNetworkManager.Instance.OnPvPSceneLoaded -= OnPvPSceneLoaded;

            EnablePvPInput();
        }

        private void EnablePvPInput()
        {
            playerInputHandler.enabled = true;
            playerInputHandler.GetComponent<UnityEngine.InputSystem.PlayerInput>().enabled = true;
            PlayerInputHandler.PvPProvider = playerInputHandler;
        }
        
        /// <summary>
        /// 异步加载场景前，先异步加载选中角色的预制体
        /// </summary>
        private IEnumerator LoadSelRolesPrefab()
        {
            if (LevelContext.IsPvPMode) yield break;

            List<RoleRegistry.RoleEntry> selectedRole = LevelContext.SelectedHeroes;
            if (selectedRole == null || selectedRole.Count <= 0) yield break;

            isLoadedSuccess = true;
            for (int i = 0; i < selectedRole.Count; i++) 
            {
                if (preloadTasks.TryGetValue(selectedRole[i], out Task<GameObject> task) && task != null) continue;
                
                task = RoleRegistry.LoadPrefab(selectedRole[i]);
                preloadTasks.Add(selectedRole[i], task);
                
                float waited = 0f;
                while (!task.IsCompleted && waited < roleLoadTimeout)
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
                
                if (!task.IsCompleted || task.IsFaulted || task.Result == null)
                {
                    isLoadedSuccess = false;
                    Debug.LogError($"[PlayerController] Failed to load role {selectedRole[i].roleName} prefab");
                    CleanHandle();
                    yield break;
                }
                Debug.Log($"[PlayerController] load prefab: {selectedRole[i].roleName}");
            }
        }

        private void InitSelectedRoles()
        {
            CleanHandle();
            StartCoroutine(InitSelectedRolesRoutine(SceneLoader.PrepareVersion));
        }

        /// <summary>
        /// 异步加载并实例化选中角色。加载期间每帧让出主线程（不能阻塞：地址化资源加载与场景切换
        /// 都需要主线程，阻塞会互相死锁），完成后由 finally 向 SceneLoader 上报就绪
        /// </summary>
        private IEnumerator InitSelectedRolesRoutine(int generation)
        {
            try
            {
                isLoadedSuccess = false;
                yield return StartCoroutine(LoadSelRolesPrefab());
                
                if (!isLoadedSuccess)
                {
                    Debug.LogWarning("[PlayerController] 角色资源未全部就绪，跳过本次角色初始化");
                    yield break;
                }

                if (generation != SceneLoader.PrepareVersion)
                {
                    Debug.LogWarning($"[PlayerController] 角色加载完成时场景已切换（gen={generation} -> {SceneLoader.PrepareVersion}），丢弃本次角色初始化");
                    yield break;
                }

                pendingStrategies = new IRoleStrategy[LevelContext.SelectedHeroes.Count];

                // 初始化角色状态
                for (int i = 0; i < LevelContext.SelectedHeroes.Count; i++)
                {
                    RoleRegistry.RoleEntry entry = LevelContext.SelectedHeroes[i];
                    
                    if (!preloadTasks.TryGetValue(entry, out Task<GameObject> task) || task == null || !task.IsCompleted || task.IsFaulted || task.Result == null)
                    {
                        Debug.LogError($"[PlayerController] 角色 {entry.roleName} 的资源未就绪，放弃本次角色初始化");
                        AbortRoleInit();
                        yield break;
                    }

                    GameObject instance = Instantiate(task.Result, transform);
                    roleInstances.Add(instance);
                    RoleContext context = instance.GetComponentInChildren<RoleContext>();

                    if (!playerDataManager.PlayerData.ownedRoles.TryGetValue(entry.template.roleName, out RoleSaveData saveData))
                    {
                        saveData = new RoleSaveData { roleName = entry.template.roleName, roleLevel = 1 };
                        entry.template.FirstLoadSaveData(saveData);
                    }
                    context.Initialize(entry.template, saveData);
                    pendingStrategies[i] = RoleFactory.CreateRoleStrategy(entry.roleName, context);
                }
                
                Debug.Log("[PlayerController] 角色实例已创建，等待关卡场景加载完成后再启用控制");
            }
            finally
            {
                if (generation == SceneLoader.PrepareVersion)
                {
                    Debug.Log($"[PlayerController] 上报准备就绪（gen={generation}）");
                    SceneLoader.ReportReady();
                }
            }
        }

        private void SceneLoaded()
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                Vector2 pos = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f));
                
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(pos.x, transform.position.y), Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
                if (hit.collider != null) pos.y = hit.point.y + 0.5f;

                transform.position = pos;
            }
            else
            {
                transform.position = Vector3.zero;
            }

            StartRoleControl();
        }

        /// <summary>
        /// 启动角色控制
        /// </summary>
        private void StartRoleControl()
        {
            if (pendingStrategies == null || roleInstances.Count <= 0) return;

            playerInputHandler.Init(pendingStrategies, roleInstances);
            
            Debug.Log($"[PlayerController] 关卡场景已就绪，启用 {pendingStrategies.Length} 个角色的控制）");
        }
        
        private void Cleanup()
        {
            AbortRoleInit();
            CleanHandle();
            if (!LevelContext.IsPvPMode) playerInputHandler.Cleanup();
        }

        private void CleanHandle()
        {
            foreach (RoleRegistry.RoleEntry entry in preloadTasks.Keys)
            {
                RoleRegistry.ReleasePrefab(entry);
            }
            preloadTasks.Clear();
        }
        
        private void AbortRoleInit()
        {
            foreach (GameObject instance in roleInstances)
            {
                if (instance != null) Destroy(instance);
            }
            roleInstances.Clear();
            pendingStrategies = null;
        }
        
        private void OnDestroy()
        {
            SceneLoader.OnPrepareLevel -= InitSelectedRoles;
            SceneLoader.OnLevelLoaded -= SceneLoaded;
            SceneLoader.OnLevelExit -= Cleanup;

            if (PvPNetworkManager.Instance != null)
                PvPNetworkManager.Instance.OnPvPSceneLoaded -= OnPvPSceneLoaded;
        }
    }
}
