using System;
using GamePlay.CharacterControllers.RoleData;
using GamePlay.CharacterControllers.RoleStrategy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.CharacterControllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private Transform visualTransform;
        private Animator animator;
        
        private IRoleStrategy[] strategies = new IRoleStrategy[2];
        private IRoleStrategy currentStrategy;
        private int currentIndex;
        private Rigidbody2D rb;
        private bool facingRight;
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = visualTransform.GetComponent<Animator>();
        }

        void Start()
        {
            facingRight = transform.lossyScale.x > 0;
            Vector3 offset = visualTransform.GetComponent<SpriteRenderer>().sprite.bounds.center;
            visualTransform.localPosition = new Vector3(-offset.x, -offset.y / 2, 0);

            RoleBaseData data = playerDataManager.PlayerData.ownedRoles[0];
            SunWuKongStrategy strategy = new ();
            strategy.Initialize(data, animator, rb);
            currentIndex = 0;
            strategies[currentIndex] = strategy;
            currentStrategy = strategies[currentIndex];
        }

        private void Update()
        {
            currentStrategy.Tick();
        }

        private void SwitchStrategy()
        {
            currentIndex = 1 - strategies.Length;
            currentStrategy = strategies[currentIndex];
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            currentStrategy.Move(direction);
            animator.SetFloat(AnimationParameters.Speed, Mathf.Abs(direction.x));
            if(direction.x > 0 && !facingRight)
            {
                Flip();
            }
            else if(direction.x < 0 && facingRight)
            {
                Flip();
            }
        }
        
        public void OnAttack(InputAction.CallbackContext context)
        {
            currentStrategy.Attack(context);
        }

        public void OnSkill1(InputAction.CallbackContext context)
        {
            currentStrategy.UseSkill1();
        }
        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        
    }
}
