using System.Collections.Generic;
using GamePlay.GameModel;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GamePlay.Role.RoleBoxCollider
{
    public class BoxColliderManager : MonoBehaviour
    {
        public enum BoxColliderName
        {
            HitBox_Normal_1,
            HitBox_Normal_2,
            HitBox_Normal_3,
            HitBox_Normal_4,
            HitBox_Normal_5,
            
            HitBox_Skill_1,
            HitBox_Skill_2,
            HitBox_Skill_3,
            HitBox_Skill_4
        }

        public enum InstantBoxColliderName
        {
            InstantBox_Skill_1,
            InstantBox_Skill_2,
            InstantBox_Skill_3,
            InstantBox_Normal_1,
            InstantBox_Normal_2,
            InstantBox_Normal_3,
            InstantBox_Normal_4,
            InstantBox_Normal_5,
        }
        
        
        private int enemyLayer;
        private RoleRuntimeData runtimeData;
        private HashSet<Collider2D> targets = new ();
        private readonly Dictionary<Collider2D, float> lastDamageTime = new ();
        
        [SerializeField] private RoleBaseData.ContinuousHitBoxData[] continuousBoxesConfig;
        [SerializeField] private RoleBaseData.InstantHitBoxData[] instantBoxesConfig;
        /// <summary>
        /// 持续检测的碰撞盒
        /// </summary>
        private readonly Dictionary<BoxColliderName, RoleBaseData.ContinuousHitBoxData> continuousBoxes = new ();
        /// <summary>
        /// 瞬时检测的碰撞盒
        /// </summary>
        private readonly Dictionary<InstantBoxColliderName, RoleBaseData.InstantHitBoxData> instantBoxes = new ();
        
        Collider2D[] enemies = new Collider2D[5];
        ContactFilter2D filter;
        public void InitRuntime(RoleRuntimeData _runtimeData)
        {
            runtimeData = _runtimeData;
        }

        private void Start()
        {
            foreach (RoleBaseData.ContinuousHitBoxData hitBox in continuousBoxesConfig)
            {
                continuousBoxes.Add(hitBox.boxColliderName, hitBox);
                
                BoxColliderReporter reporter = hitBox.collider.gameObject.AddComponent<BoxColliderReporter>();
                reporter.Init(this, hitBox.boxColliderName);
            }

            foreach (RoleBaseData.InstantHitBoxData instantBox in instantBoxesConfig)
            {
                instantBoxes.Add(instantBox.instantBoxName, instantBox);
            }
            
            enemyLayer = LayerMask.GetMask("Enemy");
            filter.SetLayerMask(enemyLayer);
            filter.useLayerMask = true;
        }

        public void EnableBox(BoxColliderName boxName)
        {
            targets.Clear();
            lastDamageTime.Clear();
            Collider2D coll2D = GetCollider(boxName);
            if (coll2D != null) coll2D.enabled = true;
        }

        public void DisableBox(BoxColliderName boxName)
        {
            targets.Clear();
            lastDamageTime.Clear();
            Collider2D coll2D = GetCollider(boxName);
            if (coll2D != null) coll2D.enabled = false;
        }
        
        public void InstantHit(InstantBoxColliderName instantBoxName)
        {
            RoleBaseData.InstantHitBoxData? instantData = GetInstantBoxCollider(instantBoxName);
            if (instantData == null) return;

            Vector2 center = (Vector2)transform.position + new Vector2(instantData.Value.offset.x, instantData.Value.offset.y);
            int count = instantData.Value.useCircle
                ? Physics2D.OverlapCircle(center, instantData.Value.radius, filter, enemies)
                : Physics2D.OverlapBox(center, instantData.Value.size, 0f, filter, enemies);

            float damage = GetDamage(instantData.Value.skillIndex);
            for (int i = 0; i < count; i++)
            {
                if (enemies[i].TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(damage);
                }
            }
#if UNITY_EDITOR
            if (count > 0)
            {
                c = center;
                s = instantData.Value.radius;
                Debug.Log(instantData.Value.instantBoxName);
            }
#endif
        }
        
#if UNITY_EDITOR
        private Vector2 c;
        private float s;
        void OnDrawGizmos()
        {
            if(s > 0)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(c, Vector3.forward, s);
            }
        }
#endif

        public void TriggerEnter2D(BoxColliderName boxName, Collider2D other)
        {
            if (!targets.Add(other)) return;
            if (!other.TryGetComponent(out IDamageable target)) return;

            RoleBaseData.ContinuousHitBoxData? data = GetContinuousHitBoxData(boxName);
            if (data == null) return;

            float damage = GetDamage(data.Value.skillIndex);
            target.TakeDamage(damage);
            lastDamageTime[other] = Time.time;
        }

        public void TriggerStay2D(BoxColliderName boxName, Collider2D other)
        {
            if (!targets.Contains(other)) return;

            RoleBaseData.ContinuousHitBoxData? data = GetContinuousHitBoxData(boxName);
            if (data == null) return;

            int skillIndex = data.Value.skillIndex;
            if (skillIndex < 0 || skillIndex >= runtimeData.skillRuntimeData.Length) return;

            float interval = runtimeData.skillRuntimeData[skillIndex].damageInterval;
            if (interval <= 0f) return;

            if (!lastDamageTime.TryGetValue(other, out float lastTime)) return;
            if (Time.time - lastTime < interval) return;

            float damage = GetDamage(skillIndex);
            if (other.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
                lastDamageTime[other] = Time.time;
            }
        }
        
        private Collider2D GetCollider(BoxColliderName boxName)
        {
            return continuousBoxes.TryGetValue(boxName, out RoleBaseData.ContinuousHitBoxData data) ? data.collider : null;
        }

        private RoleBaseData.ContinuousHitBoxData? GetContinuousHitBoxData(BoxColliderName boxName)
        {
            return continuousBoxes.TryGetValue(boxName, out RoleBaseData.ContinuousHitBoxData data) ? data : null;
        }
        private RoleBaseData.InstantHitBoxData? GetInstantBoxCollider(InstantBoxColliderName nm)
        {
            return instantBoxes.TryGetValue(nm, out RoleBaseData.InstantHitBoxData data) ? data : null;
        }

        private float GetDamage(int skillIndex)
        {
            if (runtimeData == null) return 0f;
            if (skillIndex >= 0 && skillIndex < runtimeData.skillRuntimeData.Length)
                return runtimeData.skillRuntimeData[skillIndex].damage;
            return runtimeData.damage;
        }
    }
}
