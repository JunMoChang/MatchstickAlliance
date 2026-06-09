using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.Role
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private PlayerInputHandler playerInputHandler;
        private readonly List<GameObject> roleInstances = new ();

        private static bool exists;

        private void Awake()
        {
            if (exists)
            {
                Destroy(gameObject);
                return;
            }
            exists = true;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            playerInputHandler.enabled = false;
            SceneLoader.OnPrepareLevel += InitSelectedRoles;
            SceneLoader.OnLevelLoaded += SceneLoaded;
            SceneLoader.OnLevelExit += Cleanup;
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
                context.Init(entry.template, saveData);
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
            playerInputHandler.Cleanup();
        }
    }
}
