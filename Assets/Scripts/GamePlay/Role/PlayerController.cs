using GamePlay.Role.RoleData;
using GamePlay.Role.RoleStrategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Role
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDataManager playerDataManager;
        
        private IRoleStrategy[] strategies = new IRoleStrategy[2];
        private IRoleStrategy currentStrategy;
        private int currentIndex;
        
        void Start()
        {
            RoleContext context = GetComponentInChildren<RoleContext>();
            currentStrategy = new SunWuKongStrategy();
            currentStrategy.Initialize(context);
            //playerDataManager.PlayerData.ownedRoles.Add(context.roleData);
        }

        public RoleSaveData UnlockRole(RoleName roleName)
        {
            return RoleFactory.GetRoleSaveData(roleName);
        }
        private void Update()
        {
            currentStrategy.Tick();
        }

        private void FixedUpdate()
        {
            currentStrategy.FixedTick();
        }
        private void SwitchStrategy()
        {
            currentIndex = 1 - currentIndex;
            currentStrategy = strategies[currentIndex];
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
