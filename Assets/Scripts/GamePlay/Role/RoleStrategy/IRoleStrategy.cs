using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using UnityEngine;

namespace GamePlay.Role.RoleStrategy
{
    public interface IRoleStrategy
    {
        void Initialize(RoleContext context);
        void Tick();
        void FixedTick();
        void OnMotionEvent(RoleBaseData.MotionName eventName);
        void Move(Vector2 direction);
        void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context);
        void UseSkill(int index);
        void Death();
    }
}