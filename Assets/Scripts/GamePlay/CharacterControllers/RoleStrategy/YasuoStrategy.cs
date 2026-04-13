using GamePlay.CharacterControllers.RoleData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.CharacterControllers.RoleStrategy
{
    public class YasuoStrategy : IRoleStrategy
    {
        public void Initialize(RoleBaseData data, Animator animator, Rigidbody2D rb)
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
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

        public void UseSkill1()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill2()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill3()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill4()
        {
            throw new System.NotImplementedException();
        }

        public RoleBaseData DataData { get; }
    }
}