using UnityEngine;

namespace GamePlay.CharacterControllers.RoleStrategy
{
    public interface IRoleStrategy
    {
        void Initialize(RoleData.RoleBaseData data, Animator animator, Rigidbody2D rb);
        void Tick();
        void Move(Vector2 direction);
        void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context);
        void UseSkill1();
        void UseSkill2();
        void UseSkill3();
        void UseSkill4();
        RoleData.RoleBaseData DataData { get; }  
    }
}