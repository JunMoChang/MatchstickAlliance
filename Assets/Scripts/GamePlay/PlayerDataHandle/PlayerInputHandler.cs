using System.Collections.Generic;
using GamePlay.Role.RoleStrategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.PlayerDataHandle
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInput playerInput;
        private IRoleStrategy[] strategies = new IRoleStrategy[2];
        private List<GameObject> roleInstances;
        private IRoleStrategy currentStrategy;
        private int currentIndex;

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false; 
        }
        public void Init(IRoleStrategy[] _strategies, List<GameObject> _roleInstances)
        {
            strategies = _strategies;
            currentIndex = 0;
            currentStrategy = strategies[0];
            roleInstances = _roleInstances;
            enabled = true;
            playerInput.enabled = true;
        }
        
        public void SwitchStrategy()
        {
            if(roleInstances.Count < 2) return;
            
            roleInstances[currentIndex].SetActive(false);
            
            currentIndex = 1 - currentIndex;
            roleInstances[currentIndex].SetActive(true);
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
        
        public void Cleanup()
        {
            playerInput.enabled = false;
            strategies = null;
            currentStrategy = null;
            enabled = false;
        }
    }
}