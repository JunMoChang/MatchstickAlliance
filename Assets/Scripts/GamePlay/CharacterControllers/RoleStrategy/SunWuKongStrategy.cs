using GamePlay.CharacterControllers.RoleData;
using UnityEngine;

namespace GamePlay.CharacterControllers.RoleStrategy
{
    [System.Serializable]
    public class SunWuKongStrategy : IRoleStrategy
    {
        private SunWuKongData data;
        private Animator animator;
        private Rigidbody2D rb;

        private int MaxCombos = 4;
        private int combosStep;
        private bool isAttacking;
        private bool pendingAttack;
        
        private bool facingRight;
        private float[] skillTimers = new float[4];
        private float[] skillCooldowns = new float[4];
        private bool inputLocked;

        public RoleBaseData DataData => data;
        public void Initialize(RoleBaseData roleData, Animator anim, Rigidbody2D rb2D)
        {
            data = roleData as SunWuKongData;
            animator = anim;
            rb = rb2D;
        }

        public void Tick()
        {
            CombosWindows();
        }

        public void Move(Vector2 direction)
        {
            rb.linearVelocity = new Vector2(direction.x * data.defaultSpeed,  rb.linearVelocity.y);
        }
        
        public void Attack(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (!isAttacking)
                {
                    StartCombo();
                }
                else
                {
                    pendingAttack = true;
                }
            }
        }
        private void StartCombo()
        {
            isAttacking = true;
            combosStep = 0;
            animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
            animator.SetTrigger(AnimationParameters.NormalAttack);
        }
        
        private void CombosWindows()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!isAttacking || !stateInfo.IsTag("NormalAttack") || animator.IsInTransition(0)) return;
                
            float progress = stateInfo.normalizedTime; 
            if (progress is > 0.7f and < 1f && pendingAttack)
            {
                pendingAttack = false;
                if (combosStep < MaxCombos - 1)
                {
                    combosStep++;
                    animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
                }
            }
            
            if (progress >= 1f)
            {
                animator.SetInteger(AnimationParameters.NormalCombos, -1);
                isAttacking = false;
                pendingAttack = false;
                combosStep = 0;
            }
        }
        public void OnSkills(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            switch (context.control.name)
            {
                case "1":UseSkill(0); break;
                case "2":UseSkill(1); break;
                case "3":UseSkill(2); break;
                case "4":UseSkill(3); break;
            }
        }
        private void UseSkill(int index)
        {
            if (skillTimers[index] > 0f)
            {
                Debug.Log($"Skill {index + 1} is on cooldown ({skillTimers[index]:F1}s remaining)");
                return;
            }
            skillTimers[index] = skillCooldowns[index];

            inputLocked = true;
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
    }
}