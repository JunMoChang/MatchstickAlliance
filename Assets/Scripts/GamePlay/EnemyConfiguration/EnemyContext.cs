using System;
using GamePlay.GameModel;
using GamePlay.Scene;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace GamePlay.EnemyConfiguration
{
    enum EnemyState
    {
        Idle, Chase, Attack, HitReaction, Dead
    }
    
    public class EnemyContext : MonoBehaviour, IDamageable
    {
        public float Hp { get; private set; }
        public float Damage { get; private set; }

        private Transform player;
        private EnemyData enemyData;
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private SpriteRenderer mainSpriteRenderer;
        [SerializeField] private EnemyGib gibSystem;
        private Vector2 lastHitDirection = Vector2.right;
        
        private CircleCollider2D attackBox;
        private float attackRange;
        /// <summary> 起手余量, 挥锤到命中帧间玩家可能移动</summary>
        private const float AttackLeadBonus = 0.15f;
        private readonly Collider2D[] targets = new Collider2D[8];
        
        private LayerMask groundLayer;
        private float groundDetectionDistance =  0.3f;
        private float changeDelay = 2.5f; // 状态切换延迟时间 
        private float changeTimer;
        private ContactFilter2D filter;
        
        private EnemyState state;
        private float attackCooldownTimer;
        private bool hasDealtDamageThisAttack;
        private bool isOnGround;
        private float DistanceToPlayer => Vector2.Distance(transform.position, player.position);
        private bool CooldownReady => attackCooldownTimer <= 0f;
        public event Action<Vector2> OnEnemyDied;
        public void Init(float hp, float damage, Transform playerTransform, EnemyData data)
        {
            Hp = hp;
            Damage = damage;
            player = playerTransform;
            enemyData = data;
            state = EnemyState.Idle;
            attackCooldownTimer = 0f;
            hasDealtDamageThisAttack = false;
            groundLayer =  LayerMask.GetMask("Ground");
            attackBox = GetComponentInChildren<CircleCollider2D>();
            
            if (attackBox == null)
            {
                Debug.LogError($"{name}: 缺少攻击盒", this);
                return;
            }

            attackRange = Mathf.Abs(attackBox.offset.x) + attackBox.radius + AttackLeadBonus;
            filter.SetLayerMask(LayerMask.GetMask("Role"));
            filter.useLayerMask = true;

            SnapToGround();
        }

        void Update()
        {
            if (!player || !enemyData) return;

            if (attackCooldownTimer > 0f)
                attackCooldownTimer -= Time.deltaTime;

            switch (state)
            {
                case EnemyState.Idle: IdleUpdate(); break;
                case EnemyState.Chase: ChaseUpdate(); break;
                case EnemyState.Attack: AttackUpdate(); break;
                case EnemyState.HitReaction: HitReactionUpdate(); break;
                case EnemyState.Dead: DeadUpdate(); break;
            }
        }

        void FixedUpdate()
        {
            if (state == EnemyState.Chase) ChaseMove();
        }
        
        private void IdleUpdate()
        {
            animator.SetFloat(AnimationParameters.EnemySpeed, -1f);
            
            changeTimer -= Time.deltaTime;
            if (changeTimer <= 0)
            {
                changeTimer = Random.Range(0.5f, changeDelay);
                
                float dist = DistanceToPlayer;
                if (dist <= attackRange && CooldownReady)
                {
                    ChangeState(EnemyState.Attack);
                }
                else if (dist > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
            }
        }

        private void ChaseUpdate()
        {
            float dist = DistanceToPlayer;
            if (dist <= attackRange)
            {
                ChangeState(CooldownReady ? EnemyState.Attack : EnemyState.Idle);
            }
        }

        private void ChaseMove()
        {
            float dx = player.position.x - transform.position.x;
            bool hasHorizontalOffset = Mathf.Abs(dx) > 0.1f;

            animator.SetFloat(AnimationParameters.EnemySpeed, hasHorizontalOffset ? 1f : -1f);

            if (hasHorizontalOffset)
            {
                FaceDirection(dx);

                Vector2 target = GroundConstraint(rb.position + enemyData.moveSpeed * Time.fixedDeltaTime * new Vector2(Mathf.Sign(dx), 0f));
                target.x = LevelBounds.ClampX(target.x);
                rb.MovePosition(target);
            }
        }

        private void AttackUpdate()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (!stateInfo.IsName("Attack")) return;
            if (!hasDealtDamageThisAttack && stateInfo.normalizedTime >= 0.5f)
            {
                DealDamage();
            }

            if (stateInfo.normalizedTime >= 0.95f)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        private void DealDamage()
        {
            hasDealtDamageThisAttack = true;

            if (attackBox == null) return;
            
            float facingX = transform.localScale.x < 0f ? -1f : 1f;
            Vector2 center = (Vector2)transform.position + new Vector2(attackBox.offset.x * facingX, attackBox.offset.y);
            int count = Physics2D.OverlapCircle(center, attackBox.radius, filter, targets);
            for (int i = 0; i < count; i++)
            {
                IDamageable target = targets[i].GetComponentInParent<IDamageable>();
                if (target == null) continue;

                target.TakeDamage(Damage);
            }
        }

        private void HitReactionUpdate()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (!stateInfo.IsName("HitReaction")) return;
            if (stateInfo.normalizedTime >= 0.95f)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        private void DeadUpdate()
        {
            Destroy(gameObject);
        }

        private void ChangeState(EnemyState newState)
        {
            state = newState;

            switch (newState)
            {
                case EnemyState.Attack:
                    attackCooldownTimer = enemyData.attackCooldown;
                    hasDealtDamageThisAttack = false;
                    animator.SetFloat(AnimationParameters.EnemySpeed, -1f);
                    animator.SetTrigger(AnimationParameters.Attack);
                    FacePlayer();
                    break;
                case EnemyState.HitReaction:
                    animator.SetTrigger(AnimationParameters.Hit);
                    break;
                case EnemyState.Dead:
                    DisableColliders();
                    animator.enabled = false;
                    mainSpriteRenderer.enabled = false; 
                        
                    break;
            }
        }

        private void DisableColliders()
        {
            bodyCollider.enabled = false;
            
            rb.simulated = false;
        }

        private void FaceDirection(float dirX)
        {
            if (Mathf.Approximately(dirX, 0f)) return;
            bool faceRight = dirX > 0f;
            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private void FacePlayer()                           
        {
            if (!player) return;
            float dir = player.position.x - transform.position.x;
            FaceDirection(dir);
        }
        
        public void Die()
        {
            OnEnemyDied?.Invoke(transform.position);
            Destroy(gameObject);
        }
        
        public void TakeDamage(float damage)
        {
            if (state == EnemyState.Dead) return;
            
            lastHitDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;

            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0;
                gibSystem.Explode(lastHitDirection);
                Die();
                ChangeState(EnemyState.Dead);
                return;
            }
            ChangeState(EnemyState.HitReaction);
        }
        
        private RaycastHit2D GroundDetect()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundDetectionDistance, groundLayer);
            
            if(hit.collider != null) isOnGround = true;
            else isOnGround = false;
            
            return hit;
        }
        
        /// <summary>
        /// 防止陷入地面
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private Vector2 GroundConstraint(Vector2 currentPosition)
        {
            RaycastHit2D hit = GroundDetect();

            Bounds bounds = bodyCollider.bounds;
            if (hit.collider != null)
            {
                float groundY = hit.point.y - (bounds.min.y - rb.position.y);
                currentPosition.y = groundY;
            }

            return currentPosition;
        }
        
        /// <summary>
        /// 瞬间回到地面
        /// </summary>
        private void SnapToGround()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, groundLayer);
            if (hit.collider == null) return;
            
            rb.position = new Vector2(rb.position.x, hit.point.y + (rb.position.y - bounds.min.y)); 
        }
    }
}
