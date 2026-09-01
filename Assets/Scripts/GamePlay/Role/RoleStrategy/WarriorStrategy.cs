using GamePlay.Role.RoleData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Role.RoleStrategy
{
    public class YasuoStrategy : IRoleStrategy
    {
        private RoleContext roleContext;
        
        public void Initialize(RoleContext context)
        {
            roleContext = context;
        }

        public void Tick(float deltaTime)
        {

        }

        public void FixedTick(float deltaTime)
        {

        }

        public Vector2 Move(Vector2 direction)
        {
            return Vector2.zero;
        }

        public void Attack(InputAction.CallbackContext context)
        {

        }

        public void PerformAttack()
        {

        }

        public void UseSkill(int index)
        {

        }

        public void EndAction()
        {

        }

        public void OnHit()
        {
            
        }

        public void Death()
        {
            
        }

        public void OnRespawn()
        {
     
        }
    }
}