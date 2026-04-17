using GamePlay.Role.RoleBoxCollider;
using GamePlay.Role.RoleData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Role.RoleStrategy
{
    public class YasuoStrategy : IRoleStrategy
    {
        public void Initialize(RoleContext context)
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public void FixedTick()
        {
            throw new System.NotImplementedException();
        }

        public void OnMotionEvent(RoleBaseData.MotionName eventName)
        {
            throw new System.NotImplementedException();
        }

        public void OnMotionEvent(BoxColliderManager.BoxColliderName eventName)
        {
            throw new System.NotImplementedException();
        }

        public void Move(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }

        public void Attack(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill(int index)
        {
            throw new System.NotImplementedException();
        }

        public RoleBaseData RoleData { get; }
    }
}