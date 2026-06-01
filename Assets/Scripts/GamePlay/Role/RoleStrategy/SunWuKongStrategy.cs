using System.Collections.Generic;
using GamePlay.GameModel;
using UnityEngine;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;

namespace GamePlay.Role.RoleStrategy
{
    [System.Serializable]
    public class SunWuKongStrategy : IRoleStrategy
    {
        private RoleContext roleContext;
        private MonkeyData template;
        private RoleBoxCollider.BoxColliderManager boxColliderManager;
        private Animator animator;
        private Rigidbody2D rb;
        
        private Vector2 moveInput;
        private int combosStep;
        private bool isAttacking;
        private bool pendingAttack;
        private bool facingRight = true;
        
        private RoleRuntimeData runtimeData;
        
        private Dictionary<RoleBaseData.MotionName, RoleBaseData.MotionKeyframe[]> motionKeyframes = new();
        private RoleBaseData.MotionName activeMotionName;
        private bool motionSequenceActive;
        private bool isActionActive; 
        private int motionKeyframeIndex;
        private bool hasPersistentVelocity;
        private float persistentVelocityX;
        private float persistentVelocityY;

        public void Initialize(RoleContext _context)
        {
            roleContext = _context;
            template = (MonkeyData)roleContext.Template;
            runtimeData = roleContext.RuntimeData;
            animator = roleContext.animator;
            rb = roleContext.rb;
            boxColliderManager = roleContext.boxColliderManager;

            foreach (RoleBaseData.MotionCommand command in template.motionCommands)
            {
                motionKeyframes.Add(command.motionName, command.keyframes);
            }
            
            roleContext.SetStrategy(this);
        }

        public void Tick()
        {
            CombosWindows();
            TickMotionSequence();
            foreach (RoleRuntimeData.RuntimeSkillData skillData in runtimeData.skillRuntimeData)
            {
                skillData.Tick(Time.deltaTime);
            }
        }

        public void FixedTick()
        {
            ApplyMove();
            ApplyPersistentVelocity();
        }
        
        public void Move(Vector2 direction)
        {
            moveInput = direction;
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
            if (!isActionActive) animator.SetFloat(AnimationParameters.Speed, Mathf.Abs(direction.x));
        }
        private void ApplyMove()
        {
            if (moveInput.x == 0 || isActionActive) return;
            rb.MovePosition(new Vector2(rb.position.x + moveInput.x * runtimeData.speed * Time.fixedDeltaTime, rb.position.y));
        }
        
        private void ApplyPersistentVelocity()
        {
            if (!hasPersistentVelocity) return;
            
            float dir = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * persistentVelocityX, persistentVelocityY);
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
            isActionActive = true;
            isAttacking = true;
            combosStep = 0;
            animator.SetInteger(AnimationParameters.NormalCombos, combosStep);
            animator.SetTrigger(AnimationParameters.NormalAttack);
        }
        private void CombosWindows()
        {
            if(!isAttacking) return;
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
                        isActionActive = true;
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
                    isActionActive = false;
                    motionSequenceActive = false;
                    hasPersistentVelocity = false;
                    break;
                }
            }
        }
   
        public void UseSkill(int index)
        {
            if (!runtimeData.skillRuntimeData[index].IsReady) return;
            runtimeData.skillRuntimeData[index].TriggerCooldown();
            switch (index)
            {
                case 0: Skill1Logic(); break;
                case 1: Skill2Logic(); break;
                case 2: Skill3Logic(); break;
                case 3: Skill4Logic(); break;
            }
        }
        
        private void Skill1Logic()
        {
            animator.SetTrigger(AnimationParameters.Skill_1);
        }
        private void Skill2Logic()
        {
            animator.SetTrigger(AnimationParameters.Skill_2);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, motionKeyframes[RoleBaseData.MotionName.Skill_2][0].motionData.velocity.y);
        }
        private void Skill3Logic()
        {
            animator.SetTrigger(AnimationParameters.Skill_3);
        }
        private void Skill4Logic()
        {
            boxColliderManager.EnableBox(RoleBoxCollider.BoxColliderManager.BoxColliderName.HitBox_Skill_4);
            animator.SetTrigger(AnimationParameters.Skill_4);
        }
        
        public void OnMotionEvent(RoleBaseData.MotionName eventName)
        {
            if (eventName == RoleBaseData.MotionName.SkillEnd)
            {
                motionSequenceActive = false;
                hasPersistentVelocity = false;
                isActionActive = false; 
                return;
            }
            
            isActionActive = true;
            
            if (!motionKeyframes.ContainsKey(eventName))
            {
                motionSequenceActive = false;
                hasPersistentVelocity = false;
                return;
            }
            
            activeMotionName = eventName;
            motionKeyframeIndex = 0;
            motionSequenceActive = true;
            hasPersistentVelocity = false;
        }

        private void TickMotionSequence()
        {
            if(!motionSequenceActive) return;
            if (animator.IsInTransition(0)) return;
            
            RoleBaseData.MotionKeyframe[] frames = motionKeyframes[activeMotionName];
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float progress = stateInfo.normalizedTime;
            
            while (motionKeyframeIndex < frames.Length && progress >= frames[motionKeyframeIndex].normalizedTime)
            {
                ApplyMotion(frames[motionKeyframeIndex].motionData);
                motionKeyframeIndex++;
            }
        }
        private void ApplyMotion(RoleBaseData.MotionData motion)
        {
            float dir = facingRight ? 1f : -1f;
            
            switch (motion.motionType)
            {
                case RoleBaseData.MotionData.MotionType.LinearVelocity:
                    if (motion.playerControlledDirection)
                    {
                        hasPersistentVelocity = true;
                    }
                    else
                    {
                        hasPersistentVelocity = false;
                        rb.linearVelocity = new Vector2(motion.velocity.x * dir, motion.velocity.y);
                    }
                    persistentVelocityX = motion.velocity.x;
                    persistentVelocityY = motion.velocity.y;
                    break;
                case RoleBaseData.MotionData.MotionType.MovePosition:
                    rb.MovePosition(rb.position + new Vector2(dir * motion.offset.x, motion.offset.y));
                    break;
                case RoleBaseData.MotionData.MotionType.AddForce:
                    rb.AddForce(new Vector2(dir * motion.force.x, motion.force.y), ForceMode2D.Impulse);
                    break;
                case RoleBaseData.MotionData.MotionType.GravityScale:
                    rb.gravityScale = motion.gravityScale;
                    break;
                case RoleBaseData.MotionData.MotionType.ClearVelocity:
                    hasPersistentVelocity = false;
                    break;
            }
        }
        
    }
}