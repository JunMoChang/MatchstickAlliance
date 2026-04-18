using System.Collections.Generic;
using UnityEngine;
using GamePlay.CharacterControllers;
using GamePlay.Role.RoleData;

namespace GamePlay.Role.RoleStrategy
{
    [System.Serializable]
    public class SunWuKongStrategy : IRoleStrategy
    {
        private RoleContext roleContext;
        private SunWuKongData template;
        private RoleBoxCollider.BoxColliderManager boxColliderManager;
        private Animator animator;
        private Rigidbody2D rb;
        
        private Vector2 moveInput;
        private int combosStep;
        private bool isAttacking;
        private bool pendingAttack;
        private bool facingRight = true;
        
        private float[] skillTimers = new float[4];
        private bool skill2Active;
 
        Dictionary<RoleBaseData.MotionName, RoleBaseData.MotionData[]> motionData = new ();
        public void Initialize(RoleContext _context)
        {
            roleContext = _context;
            template = (SunWuKongData)roleContext.Template;
            animator = roleContext.animator;
            rb = roleContext.rb;
            boxColliderManager = roleContext.boxColliderManager;

            foreach (RoleBaseData.MotionCommand command in template.motionCommands)
            {
                motionData.Add(command.motionName, command.motionData);
            }
            roleContext.SetStrategy(this);
        }

        public void Tick()
        {
            CombosWindows();
            UpdateCooldowns();
            UpdateSkill2();
        }

        public void FixedTick()
        {
            ApplyMove();
        }
        
        public void Move(Vector2 direction)
        {
            moveInput = direction;
            animator.SetFloat(AnimationParameters.Speed, Mathf.Abs(direction.x));
            if (direction.x > 0 && !facingRight)
            {
                facingRight = true;
                roleContext.Flip(facingRight);
            }
            else if (direction.x < 0 && facingRight)
            {
                facingRight = false;
                roleContext.Flip(facingRight);
            }
        }
        private void ApplyMove()
        {
            if (moveInput.x != 0)
                rb.MovePosition(new Vector2(rb.position.x + moveInput.x * template.defaultSpeed * Time.fixedDeltaTime, rb.position.y));
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
            switch (progress)
            {
                case > 0.75f and <= 1f when pendingAttack:
                {
                    pendingAttack = false;
                    if (combosStep < template.maxCombos - 1)
                    {
                        boxColliderManager.DisableBox((RoleBoxCollider.BoxColliderManager.BoxColliderName)combosStep);
                        combosStep++;
                        animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
                    }
                    break;
                }
                case >= 1f:
                {
                    animator.SetInteger(AnimationParameters.NormalCombos, -1);
                    isAttacking = false;
                    pendingAttack = false;
                    combosStep = 0;
                    break;
                }
            }
        }
   
        public void UseSkill(int index)
        {
            if (skillTimers[index] > 0f) return;
            skillTimers[index] = template.skillCooldowns[index];
            switch (index)
            {
                case 0:
                    Skill1Logic(index);
                    break;
                case 1:
                    Skill2Logic(index);
                    break;
                case 2:
                    Skill3Logic(index);
                    break;
                case 3:
                    Skill4Logic(index);
                    break;
            }
        }
        private void Skill1Logic(int index)
        {
            skillTimers[index] = template.skillCooldowns[index];
            animator.SetTrigger(AnimationParameters.Skill_1);
        }
        private void Skill2Logic(int index)
        {
            skill2Active = true;
            skillTimers[index] = template.skillCooldowns[index];
            animator.SetTrigger(AnimationParameters.Skill_2);
            rb.linearVelocity = new Vector2(template.skill2JumpForceX, template.skill2JumpForceY);
        }
        private void UpdateSkill2()
        {
            if (!skill2Active) return;
    
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (rb.linearVelocityY <= 0)
            {
                rb.gravityScale = 0.15f;
                if (stateInfo.normalizedTime >= 0.6f)
                {
                    rb.gravityScale = 1;
                    rb.linearVelocity = new Vector2(0, -template.skill2JumpForceY);
                    skill2Active = false;
                }
            }
        }
        private void Skill3Logic(int index)
        {
            skillTimers[index] = template.skillCooldowns[index];
            animator.SetTrigger(AnimationParameters.Skill_3);
        }
        private void Skill4Logic(int index)
        {
            skillTimers[index] = template.skillCooldowns[index];
            boxColliderManager.EnableBox(RoleBoxCollider.BoxColliderManager.BoxColliderName.HitBox_Skill_4);
            animator.SetTrigger(AnimationParameters.Skill_4);
        }
        
        public void OnMotionEvent(RoleBaseData.MotionName eventName)
        {
            if(!motionData.TryGetValue(eventName, out RoleBaseData.MotionData[] motions)) return;

            foreach (RoleBaseData.MotionData motion in motions)
            {
                ApplyMotion(motion);
            }
            
        }
        private void ApplyMotion(RoleBaseData.MotionData motion)
        {
            float dir = facingRight ? 1f : -1f;
            
            switch (motion.motionType)
            {
                case RoleBaseData.MotionData.MotionType.LinearVelocity:
                    rb.linearVelocity = new Vector2(motion.velocity.x * dir, motion.velocity.y);
                    break;
                case RoleBaseData.MotionData.MotionType.MovePosition:
                    rb.MovePosition(rb.position + new Vector2(dir * motion.offset.x, motion.offset.y));
                    break;
                case RoleBaseData.MotionData.MotionType.AddForce:
                    rb.AddForce(new Vector2(dir * motion.force.x, motion.force.y), ForceMode2D.Impulse);
                    break;
            }
        }
        
        private void UpdateCooldowns()
        {
            for (int i = 0; i < skillTimers.Length; i++)
            {
                if (skillTimers[i] > 0f) skillTimers[i] -= Time.deltaTime;
            }
        }
    }
}