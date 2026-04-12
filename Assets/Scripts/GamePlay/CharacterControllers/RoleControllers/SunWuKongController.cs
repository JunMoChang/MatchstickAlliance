using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterControllers.RoleControllers
{ 
    [RequireComponent(typeof(Rigidbody2D))]
    public class SunWuKongController : MonoBehaviour
    {
        [SerializeField] private Transform visualTransform;
        [Header("Movement")]
        public float moveSpeed = 2f;
        public float jumpForce = 14f;
        public float dashSpeed = 18f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.8f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;

        [Header("Combat")]
        public float attackComboWindow = 2f;
        public float skill1Cooldown = 5f;
        public float skill2Cooldown = 8f;
        public float skill3Cooldown = 12f;
        public float skill4Cooldown = 20f;

        [Header("Skill Cooldown UI (optional)")]
        // Assign UnityEngine.UI.Image components with fillAmount for cooldown arcs
        public UnityEngine.UI.Image skill1CooldownUI;
        public UnityEngine.UI.Image skill2CooldownUI;
        public UnityEngine.UI.Image skill3CooldownUI;
        public UnityEngine.UI.Image skill4CooldownUI;

        private Rigidbody2D rb;
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;

        // Movement
        private PlayerInput roleInput;
        private Vector2 moveInput;
        private bool facingRight = true;
        private bool isGrounded;

        // Dash
        private bool isDashing;
        private float dashTimer;
        private float dashCooldownTimer;

        // Attack combo
        private int comboStep;          // 0 = ready, 1 = first hit landed, 2 = second
        private float comboTimer = 0f;
        private bool isAttacking;

        // Skill cooldown timers (remaining time)
        private float[] skillTimers = new float[4];
        private float[] skillCooldowns;

        // Blocking all input during a locked animation (skills, certain attacks)
        private bool inputLocked;
        // ─────────────────────────────────────────────
        //  UNITY LIFECYCLE
        // ─────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            skillCooldowns = new [] { skill1Cooldown, skill2Cooldown,
                skill3Cooldown, skill4Cooldown };
        }

        void Start()
        {
            Vector3 offset = sr.sprite.bounds.center;
            visualTransform.localPosition = new Vector3(-offset.x, -offset.y / 2, 0);
        }
        private void Update()
        {
            if (inputLocked) return;

            //CheckGrounded();
            HandelAttackWindow();
            UpdateCooldownTimers();
            UpdateAnimatorParams();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                rb.linearVelocity = new Vector2((facingRight ? 1f : -1f) * dashSpeed, rb.linearVelocity.y);
            }
            else if (!isAttacking)
            {
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.85f, rb.linearVelocity.y);
            }
        }
        
        private void CheckGrounded()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
            }

            anim.SetBool("IsGrounded", isGrounded);
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>(); 
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed,  rb.linearVelocity.y);
            
            if (moveInput.x > 0 && !facingRight) Flip();
            else if (moveInput.x < 0 && facingRight) Flip();
            
            anim.SetFloat(AnimationParameters.Speed, Mathf.Abs(moveInput.x));
            
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
        
        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetTrigger("Jump");
            }
        }

        private void HandelAttackWindow()
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (isAttacking && stateInfo.IsTag("NormalAttack") && !anim.IsInTransition(0))
            {
                process = stateInfo.normalizedTime; 
                if (process >= 1)
                {
                    anim.SetInteger(AnimationParameters.NormalCombos, -1);
                    isAttacking = false;
                    comboStep = 0;
                    process = 0;
                }
            }
        }

        private float process;
        public void OnNormalAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isAttacking = true;
                if (comboStep == 0)
                {
                    anim.SetTrigger(AnimationParameters.NormalAttack);
                    anim.SetInteger(AnimationParameters.NormalCombos, comboStep++);
                }
                switch (process)
                {
                    case < 1 when process > 0.5f && isAttacking:
                        anim.SetInteger(AnimationParameters.NormalCombos, comboStep++);
                        break;
                }
            }
        }
        
        public void OnSkills(InputAction.CallbackContext context)
        {
            switch (context.control.name)
            {
                case "1":UseSkill(0, AnimationParameters.Skill_1); break;
                case "2":UseSkill(1, AnimationParameters.Skill_2); break;
                case "3":UseSkill(2, AnimationParameters.Skill_3); break;
                case "4":UseSkill(3, AnimationParameters.Skill_4); break;
            }
        }

        private void UseSkill(int index, int trigger)
        {
            if (skillTimers[index] > 0f)
            {
                Debug.Log($"Skill {index + 1} is on cooldown ({skillTimers[index]:F1}s remaining)");
                return;
            }

            skillTimers[index] = skillCooldowns[index];
            anim.SetTrigger(trigger);

            // Lock input for skill animation duration
            StartCoroutine(LockInputForSkill(index));
        }

        // Call this from an Animation Event at the "action frame" of each skill animation
        // to apply damage, spawn effects, etc.
        public void OnSkillActionFrame(int skillIndex)
        {
            // TODO: Add your skill effect logic here
            // e.g., spawn projectile, area damage, summon, etc.
            Debug.Log($"Skill {skillIndex + 1} action frame — apply effect here!");
        }

        // Call from Animation Event at the END of each skill animation
        public void OnSkillAnimationEnd()
        {
            inputLocked = false;
        }

        // ─────────────────────────────────────────────
        //  COOLDOWN MANAGEMENT
        // ─────────────────────────────────────────────

        private void UpdateCooldownTimers()
        {
            UnityEngine.UI.Image[] uis = { skill1CooldownUI, skill2CooldownUI,
                skill3CooldownUI, skill4CooldownUI };

            for (int i = 0; i < 4; i++)
            {
                if (skillTimers[i] > 0f)
                {
                    skillTimers[i] -= Time.deltaTime;
                    if (skillTimers[i] < 0f) skillTimers[i] = 0f;
                }

                // Update UI fill if assigned
                if (uis[i] != null)
                {
                    uis[i].fillAmount = skillTimers[i] / skillCooldowns[i];
                }
            }
        }

        // ─────────────────────────────────────────────
        //  ANIMATOR PARAMETER SYNC
        // ─────────────────────────────────────────────

        private void UpdateAnimatorParams()
        {
            // Vertical velocity can drive Jump/Fall blend tree if you use one
            // anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        }

        // ─────────────────────────────────────────────
        //  DAMAGE / DEATH  (call from external script)
        // ─────────────────────────────────────────────

        /// <summary>Called by enemy hit detection or projectile scripts.</summary>
        public void TakeDamage(float amount)
        {
            // TODO: reduce HP, update health bar UI
            anim.SetTrigger("Hurt");
            Debug.Log($"Player took {amount} damage.");
        }

        public void Die()
        {
            anim.SetBool("IsDead", true);
            inputLocked = true;
            rb.linearVelocity = Vector2.zero;
            // TODO: trigger death screen / game over
        }

        // ─────────────────────────────────────────────
        //  COROUTINES
        // ─────────────────────────────────────────────

        /// <summary>
        /// Locks player input for the duration of a skill animation.
        /// Relies on OnSkillAnimationEnd() being called via Animation Event,
        /// OR falls back to a timer if you haven't set up Animation Events yet.
        /// </summary>
        private IEnumerator LockInputForSkill(int skillIndex)
        {
            inputLocked = true;

            // Fallback timer: automatically unlock after 2 seconds
            // Replace with Animation Event calls for precision timing
            yield return new WaitForSeconds(2f);

            if (inputLocked) inputLocked = false;
        }

        // ─────────────────────────────────────────────
        //  GIZMOS (Editor helper)
        // ─────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}