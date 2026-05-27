using System;
using GamePlay.GameModel;
using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
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
        private enum EnemyState { Idle, Chase, Attack, HitReaction, Dead }
        private EnemyState state;
        private float attackCooldownTimer;
        private bool hasDealtDamageThisAttack;
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
            
            float dist = DistanceToPlayer;
            if (dist <= enemyData.attackRange && CooldownReady)
                TransitionTo(EnemyState.Attack);
            else if(dist > enemyData.attackRange) TransitionTo(EnemyState.Chase);
        }

        private void ChaseUpdate()
        {
            float dist = DistanceToPlayer;
            if (dist <= enemyData.attackRange)
            {
                TransitionTo(CooldownReady ? EnemyState.Attack : EnemyState.Idle);
            }
        }

        private void ChaseMove()
        {
            Vector2 dir = (player.position - transform.position).normalized;
            FaceDirection(dir.x);
            
            animator.SetFloat(AnimationParameters.EnemySpeed, 1f);
        
            rb.MovePosition(rb.position + enemyData.moveSpeed * Time.fixedDeltaTime * dir);
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
                TransitionTo(EnemyState.Idle);
            }
        }

        private void DealDamage()
        {
            hasDealtDamageThisAttack = true;
            if (DistanceToPlayer <= enemyData.attackRange + 0.5f)
            {
                IDamageable target = player.GetComponentInChildren<IDamageable>();
                target?.TakeDamage(Damage);
            }
        }

        private void HitReactionUpdate()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (!stateInfo.IsName("HitReaction")) return;
            if (stateInfo.normalizedTime >= 0.95f)
            {
                TransitionTo(EnemyState.Idle);
            }
        }

        private void DeadUpdate()
        {
            Destroy(gameObject);
        }

        private void TransitionTo(EnemyState newState)
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
                TransitionTo(EnemyState.Dead);
                return;
            }
            TransitionTo(EnemyState.HitReaction);
        }
    }
}
