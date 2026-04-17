using System.Collections.Generic;
using GamePlay.Role.RoleData;
using UnityEditor;
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
        
        [System.Serializable]
        public struct HitBoxData
        {
            public BoxColliderName boxColliderName;
            public Collider2D collider;
        }
        
        private int enemyLayer;
        private HashSet<Collider2D> targets = new ();
        [SerializeField] private HitBoxData[] hitBoxes;
        [SerializeField] private RoleBaseData.InstantHitData[] instantBoxesConfig;
        private readonly Dictionary<BoxColliderName, HitBoxData> colliderBoxes = new ();
        private readonly Dictionary<InstantBoxColliderName, RoleBaseData.InstantHitData> instantBoxes = new ();
        private void Start()
        {
            enemyLayer = LayerMask.GetMask("Enemy");
            foreach (HitBoxData hitBox in hitBoxes)
            {
                colliderBoxes.Add(hitBox.boxColliderName, hitBox);
                
                BoxColliderReporter reporter = hitBox.collider.gameObject.AddComponent<BoxColliderReporter>();
                reporter.SetBoxColliderManager(this);
            }

            foreach (RoleBaseData.InstantHitData instantBox in instantBoxesConfig)
            {
                instantBoxes.Add(instantBox.instantBoxName, instantBox);
            }
        }

        public void EnableBox(BoxColliderName nm)
        {
            targets.Clear();
            Collider2D c = GetCollider(nm);
            if (c != null) c.enabled = true;
        }

        public void DisableBox(BoxColliderName nm)
        {
            targets.Clear();
            Collider2D c = GetCollider(nm);
            if (c != null) c.enabled = false;
        }
        
        public void InstantHit(InstantBoxColliderName instantBoxName)
        {
            RoleBaseData.InstantHitData? instantData = GetInstantBoxCollider(instantBoxName);
            if (instantData == null) return;
            
            Vector2 center = (Vector2)transform.position + new Vector2(instantData.Value.offset.x, instantData.Value.offset.y);
            Collider2D[] hits = instantData.Value.useCircle
                ? Physics2D.OverlapCircleAll(center, instantData.Value.radius, enemyLayer)
                : Physics2D.OverlapBoxAll(center, instantData.Value.size, 0, enemyLayer);
            if (hits.Length > 0)
            {
                c = center;
                s = instantData.Value.radius;
                Debug.Log(instantData.Value.instantBoxName);
            }
        }

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
        public void TriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("敌人受到攻击");
            }
        }

        public void TriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("敌人受到攻击");
            }
        }
        
        private Collider2D GetCollider(BoxColliderName nm)
        {
            return colliderBoxes.TryGetValue(nm, out HitBoxData data) ? data.collider : null;
        }
        private RoleBaseData.InstantHitData? GetInstantBoxCollider(InstantBoxColliderName nm)
        {
            return instantBoxes.TryGetValue(nm, out RoleBaseData.InstantHitData data) ? data : null;
        }
    }
}
