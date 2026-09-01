using System.Collections.Generic;
using Fusion;
using GamePlay.GameModel.Level;
using GamePlay.PvP;
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

        /// <summary>待发送给 Fusion 的输入</summary>
        private PvPInput pendingInput;

        /// <summary>PvP 模式输入提供者</summary>
        public static PlayerInputHandler PvPProvider { get; set; }

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        void OnEnable()
        {
            if (LevelContext.IsPvPMode) PvPProvider = this;
        }

        void OnDisable()
        {
            if (PvPProvider == this) PvPProvider = null;
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
            if (roleInstances.Count < 2) return;

            roleInstances[currentIndex].SetActive(false);

            currentIndex = 1 - currentIndex;
            roleInstances[currentIndex].SetActive(true);
            currentStrategy = strategies[currentIndex];
        }

        private void Update()
        {
            if (LevelContext.IsPvPMode) return;
            
            currentStrategy?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (LevelContext.IsPvPMode) return;
            
            currentStrategy?.FixedTick(Time.fixedDeltaTime);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            pendingInput.moveDirection = direction;
            
            currentStrategy?.Move(direction);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            pendingInput.attack = context.started || context.performed;
            
            if (!LevelContext.IsPvPMode) currentStrategy?.Attack(context);
        }

        public void OnSkill1(InputAction.CallbackContext context) => HandleSkill(context, 0, 1);
        public void OnSkill2(InputAction.CallbackContext context) => HandleSkill(context, 1, 2);
        public void OnSkill3(InputAction.CallbackContext context) => HandleSkill(context, 2, 3);
        public void OnSkill4(InputAction.CallbackContext context) => HandleSkill(context, 3, 4);

        private void HandleSkill(InputAction.CallbackContext context, int skillIndex, byte fusionIndex)
        {
            if (context.started)
            {
                pendingInput.skillIndex = fusionIndex;
                pendingInput.skillTriggered = true;
            }
            if (!LevelContext.IsPvPMode) currentStrategy?.UseSkill(skillIndex);
        }

        public void Cleanup()
        {
            playerInput.enabled = false;
            strategies = null;
            currentStrategy = null;
            enabled = false;
        }

        /// <summary>
        /// 将本地输入填入 Fusion NetworkInput
        /// </summary>
        public void OnFusionInput(NetworkInput input)
        {
            input.Set(pendingInput);
            pendingInput.skillTriggered = false;
            pendingInput.skillIndex = 0;
        }
    }
}
