using System;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Role
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private RoleRegistry roleRegistry; 
        
        private readonly IRoleStrategy[] strategies = new IRoleStrategy[2];
        private IRoleStrategy currentStrategy;
        private int currentIndex;

        private void Start()
        {
            RoleSaveData saveData = playerDataManager.PlayerData.ownedRoles[0];
            RoleRegistry.RoleEntry? entry = roleRegistry.GetEntry(saveData.roleName);
            GameObject instance = Instantiate(entry.Value.prefab, transform);
            RoleContext context = instance.GetComponentInChildren<RoleContext>();
            context.Init(entry.Value.template, saveData);
            currentIndex = 0;
            strategies[0] = RoleFactory.CreateRoleStrategy(saveData.roleName, context);
            currentStrategy = strategies[0];
            Debug.Log(strategies[0]);
        }

        public void InitSelectedRoles(RoleSaveData[] selectedRoles)
        {
            for (int i = 0; i < selectedRoles.Length; i++)
            {
                RoleSaveData saveData = selectedRoles[i];
                RoleRegistry.RoleEntry? entry = roleRegistry.GetEntry(saveData.roleName);
                if (entry == null) continue;
                
                GameObject instance = Instantiate(entry.Value.prefab, transform);
                RoleContext context = instance.GetComponent<RoleContext>();
                context.Init(entry.Value.template, saveData);
                
                strategies[i] = RoleFactory.CreateRoleStrategy(saveData.roleName, context);
            }
            
            currentIndex = 0;
            currentStrategy = strategies[0];
        }

        public void UnlockRole(RoleSaveData saveData)
        {
            playerDataManager.PlayerData.ownedRoles.Add(saveData);
        }
        private void SwitchStrategy()
        {
            currentIndex = 1 - currentIndex;
            currentStrategy = strategies[currentIndex];
        }
        
        private void Update()
        {
            currentStrategy.Tick();
        }

        private void FixedUpdate()
        {
            currentStrategy.FixedTick();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            currentStrategy.Move(direction);
        }
        
        public void OnAttack(InputAction.CallbackContext context)
        {
            currentStrategy.Attack(context);
        }
        public void OnSkill1(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill(0);
        }
        
        public void OnSkill2(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill(1);
        }
        public void OnSkill3(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill(2);
        }
        public void OnSkill4(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill(3);
        }
    }
}
