using GamePlay.CharacterControllers.RoleControllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.CharacterControllers
{
    public class PlayerController : MonoBehaviour
    {
        private Transform visualTransform;
        
        private IRoleStrategy[] strategies = new IRoleStrategy[2];
        private IRoleStrategy currentStrategy;
        private int currentIndex;
        private Rigidbody2D rb;
        private void Awake()
        {
            rb =  GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            currentIndex = 0;
            currentStrategy = strategies[currentIndex];
        }
        
        private void SwitchStrategy()
        {
            currentIndex = 1 - strategies.Length;
            currentStrategy = strategies[currentIndex];
        }
        private void Move(InputAction.CallbackContext context)
        {
            currentStrategy.Move(context.ReadValue<Vector2>(), rb);
        }
        private void OnAttack(InputAction.CallbackContext context)
        {
            currentStrategy.Attack();
        }

        private void OnSkill1(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill1();
        }
        
        
    }
}
