using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.Role
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance {get; private set;}
        
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private RoleRegistry roleRegistry;
        [SerializeField] private PlayerInputHandler playerInputHandler;
        private readonly List<GameObject> roleInstances = new ();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        
        private void Start()
        {
            playerInputHandler.enabled = false;
            SceneLoader.OnPrepareLevel += InitSelectedRoles;
            SceneLoader.OnLevelLoaded += SceneLoaded;
            SceneLoader.OnLevelExit += Cleanup;
            /*RoleSaveData saveData = playerDataManager.PlayerData.ownedRoles[0];
            RoleRegistry.RoleEntry? entry = roleRegistry.GetEntry(saveData.roleName);
            GameObject instance = Instantiate(entry.Value.prefab, transform);
            RoleContext context = instance.GetComponentInChildren<RoleContext>();
            context.Init(entry.Value.template, saveData);
            currentIndex = 0;
            strategies[0] = RoleFactory.CreateRoleStrategy(saveData.roleName, context);
            currentStrategy = strategies[0];
            Debug.Log(strategies[0]);*/
        }
        
        private void InitSelectedRoles()
        {
            List<RoleRegistry.RoleEntry> selectedRole = LevelContext.Instance.selectedHeroes;

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
                }
                context.Init(entry.template, saveData);
                strategies[i] = RoleFactory.CreateRoleStrategy(entry.roleName, context);
            }
            
            playerInputHandler.Init(strategies);
            
        }

        private void SceneLoaded()
        {
            if(SpawnPoint.Instance != null) transform.position = SpawnPoint.Instance.transform.position;
        }

        public void UnlockRole(RoleSaveData saveData)
        {
            bool isRepeat = playerDataManager.PlayerData.ownedRoles.TryAdd(saveData.roleName, saveData);
            Debug.Log($"是否重复添加:{isRepeat}");
        }
        
        private void Cleanup()
        {
            if(roleInstances.Count <= 0) return;
            
            foreach (GameObject instance in roleInstances)
            {
                if (instance != null) Destroy(instance);
            }
            roleInstances.Clear();
            playerInputHandler.Cleanup();
        }
    }
}
