using System.Collections.Generic;
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
        private readonly List<GameObject> roleInstances = new ();

        private static bool exists;

        private void Awake()
        {
            if (!LevelContext.IsPvPMode)
            {
                if (exists)
                {
                    Destroy(gameObject);
                    return;
                }
                exists = true;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
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
                // 先退订再订阅：多次进出大厅会重复订阅导致回调累积
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

        private void InitSelectedRoles()
        {
            List<RoleRegistry.RoleEntry> selectedRole = LevelContext.SelectedHeroes;

            IRoleStrategy[] strategies = new IRoleStrategy[selectedRole.Count];
            for (int i = 0; i < selectedRole.Count; i++)
            {
                RoleRegistry.RoleEntry entry = selectedRole[i];
                GameObject instance = Instantiate(entry.prefab, transform);
                roleInstances.Add(instance);
                RoleContext context = instance.GetComponentInChildren<RoleContext>();

                if (!playerDataManager.PlayerData.ownedRoles.TryGetValue(entry.template.roleName, out RoleSaveData saveData))
                {
                    saveData = new RoleSaveData { roleName = entry.template.roleName, roleLevel = 1 };
                    entry.template.FirstLoadSaveData(saveData);
                }
                context.Initialize(entry.template, saveData);
                strategies[i] = RoleFactory.CreateRoleStrategy(entry.roleName, context);
            }

            playerInputHandler.Init(strategies, roleInstances);
        }

        private void SceneLoaded()
        {
            SpawnPoint sp = FindAnyObjectByType<SpawnPoint>();
            if(sp != null) transform.position = sp.transform.position;
        }

        private void Cleanup()
        {
            if(roleInstances.Count <= 0) return;

            foreach (GameObject instance in roleInstances)
            {
                if (instance != null) Destroy(instance);
            }
            roleInstances.Clear();

            if (!LevelContext.IsPvPMode) playerInputHandler.Cleanup();
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
