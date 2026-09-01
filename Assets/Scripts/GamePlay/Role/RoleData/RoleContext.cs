using System;
using GamePlay.GameModel;
using GamePlay.GameModel.Level;
using GamePlay.Role.RoleBoxCollider;
using GamePlay.Role.RoleData.BaseData;
using GamePlay.Role.RoleStrategy;
using UnityEngine;

namespace GamePlay.Role.RoleData
{
    public class RoleContext : MonoBehaviour, IDamageable
    {
        public RoleBaseData Template { get; private set; }
        public RoleSaveData SaveData { get; private set; }
        public RoleRuntimeData RuntimeData { get; private set; }
        [SerializeField] public Animator animator;
        [SerializeField] public Rigidbody2D rb;
        [SerializeField] public BoxColliderManager boxColliderManager;

        [Header("地面检测")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.3f;
        public LayerMask GroundLayer => groundLayer;
        /// <summary> 地面检测距离 </summary>
        public float GroundCheckDistance => groundCheckDistance;

        private IRoleStrategy strategy;
        public IRoleStrategy Strategy => strategy;

        void Start()
        {
            groundLayer = LayerMask.GetMask("Ground");

            rb.bodyType = RigidbodyType2D.Kinematic;

            Vector3 offset = transform.GetComponent<SpriteRenderer>().sprite.bounds.center;
            transform.localPosition = new Vector3(-offset.x, -offset.y / 2, 0);

            CapsuleCollider2D bodyCollider = GetComponent<CapsuleCollider2D>();
            if (bodyCollider != null)
            {
                bodyCollider.excludeLayers = LevelContext.IsPvPMode ?
                    LayerMask.GetMask("Enemy", "Opponent") : LayerMask.GetMask("Enemy");
            }
        }

#if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            if (rb == null) return;
            CapsuleCollider2D bodyCollider = GetComponent<CapsuleCollider2D>();
            if (bodyCollider == null) return;
            
            Bounds bounds = bodyCollider.bounds;
            Vector3 origin = new Vector3(bounds.center.x, bounds.min.y, 0f);
            float rayDistance = groundCheckDistance;
            
            LayerMask checkMask = groundLayer == 0 ? LayerMask.GetMask("Ground") : groundLayer;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, checkMask);
            Gizmos.color = hit.collider != null ? Color.green : Color.yellow;
            Gizmos.DrawLine(origin, origin + Vector3.down * rayDistance);
            if (hit.collider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.05f);
            }
        }
#endif

        public void Initialize(RoleBaseData _template, RoleSaveData _saveData)
        {
            Template = _template;
            SaveData = _saveData;
            RuntimeData = new RoleRuntimeData();
            RuntimeData.Initialize(_saveData, _template);
            boxColliderManager.InitRuntime(RuntimeData);
        }
        public void SetStrategy(IRoleStrategy s)
        {
            strategy = s;
        }

        /// <summary>
        /// Play 模式调参辅助：F5重载动作配置。
        /// 结构性修改（增删 keyframe/命令）后使用
        /// </summary>
        [ContextMenu("重载动作配置")]
        public void ReloadActionConfig()
        {
            strategy?.ReloadConfig();
            Debug.Log("[RoleContext] 动作配置已重载");
        }

        /// <summary>
        /// 动作结束事件（aseprite设置帧事件）
        /// </summary>
        public void EndAction()
        {
            strategy?.EndAction();
        }

        public void Flip(bool facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            rb.transform.localScale = scale;
        }

        public void TakeDamage(float damage)
        {
            if (RuntimeData == null) return;
            if (RuntimeData.currentHealth <= 0) return;

            RuntimeData.currentHealth -= damage;

            // 普攻/技能动作进行中免疫受击打断（霸体），仅扣血；死亡判定不受此限制
            if (strategy != null && !strategy.IsUninterruptible)
            {
                strategy.OnHit();
            }

            if (RuntimeData.currentHealth <= 0)
            {
                RuntimeData.currentHealth = 0;
                strategy?.Death();
            }
        }
    }
}