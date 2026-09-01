using System.Collections.Generic;
using GamePlay.GameModel;
using GamePlay.GameModel.Level;
using GamePlay.PvP;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;

using UnityEngine;

namespace GamePlay.Role.RoleBoxCollider
{
    public class BoxColliderManager : MonoBehaviour
    {
        /// <summary> 攻击盒标识 </summary>
        public enum HitBoxName
        {
            Normal_1, Normal_2, Normal_3, Normal_4, Normal_5,
            Skill_1, Skill_2, Skill_3, Skill_4
        }
        
        /// <summary> 瞬时检测命中窗口去重 </summary>
        private readonly HashSet<Collider2D> instantWindowTargets = new();
        /// <summary> 持续检测去重（damageInterval = 0 时每目标仅一次） </summary>
        private readonly HashSet<Collider2D> continuousTargets = new();
        /// <summary> 持续盒间隔伤害时间戳 </summary>
        private readonly Dictionary<Collider2D, float> intervalLastTime = new();
        
        private readonly Collider2D[] enemies = new Collider2D[32];
        private ContactFilter2D filter;
        private RoleRuntimeData runtimeData;
        
        private int enemyLayer;
        private bool isPvPClient;

        public void InitRuntime(RoleRuntimeData _runtimeData)
        {
            runtimeData = _runtimeData;
        }

        private void Start()
        {
            if (LevelContext.IsPvPMode)
            {
                PvPNetworkManager nm = PvPNetworkManager.Instance;
                isPvPClient = nm == null || nm.Runner == null || !nm.Runner.IsServer;
                enemyLayer = LayerMask.GetMask("Opponent");
            }
            else
            {
                enemyLayer = LayerMask.GetMask("Enemy");
            }

            filter.SetLayerMask(enemyLayer);
            filter.useLayerMask = true;
        }

        /// <summary>
        /// 瞬时检测窗口内每帧调用，同一目标整个窗口只命中一次
        /// </summary>
        public void InstantHitWindow(RoleBaseData.HitBoxKeyframe keyframe)
        {
            PerformQuery(keyframe.boxData, instantWindowTargets, false);
        }

        /// <summary>
        /// 清空瞬时检测窗口去重集
        /// </summary>
        public void ClearInstantWindow()
        {
            instantWindowTargets.Clear();
        }

        /// <summary>
        /// 持续检测窗口内每帧调用, 伤害频率由 技能数据damageInterval 控制
        /// </summary>
        public void ContinuousHitWindow(RoleBaseData.HitBoxKeyframe keyframe)
        {
            PerformQuery(keyframe.boxData, null, true);
        }

        /// <summary>
        /// 清空持续盒去重与间隔状态
        /// </summary>
        public void ClearContinuousState()
        {
            continuousTargets.Clear();
            intervalLastTime.Clear();
        }

        /// <summary>
        /// 满足条件时实施伤害
        /// </summary>
        /// <param name="boxData">检测盒几何配置</param>
        /// <param name="hitFilter">非空时同一目标只命中一次</param>
        /// <param name="intervalDamage">为true时按伤害间隔持续伤害</param>
        private void PerformQuery(RoleBaseData.HitBoxData boxData, HashSet<Collider2D> hitFilter, bool intervalDamage)
        {
            if (isPvPClient) return;

            float facing = transform.lossyScale.x < 0f ? -1f : 1f;
            Vector2 center = (Vector2)transform.position + new Vector2(boxData.offset.x * facing, boxData.offset.y);
            int count = boxData.useCircle ? Physics2D.OverlapCircle(center, boxData.radius, filter, enemies)
                : Physics2D.OverlapBox(center, boxData.size, 0f, filter, enemies);

            float damage = GetDamage(boxData.skillIndex);
            int resultCount = Mathf.Min(count, enemies.Length);
            for (int i = 0; i < resultCount; i++)
            {
                if (enemies[i].transform.IsChildOf(transform)) continue;//排除自己
                if (hitFilter != null && !hitFilter.Add(enemies[i])) continue;//排除攻击过的敌人
                if (intervalDamage && !TryIntervalDamage(enemies[i], boxData.skillIndex)) continue;//计算伤害间隔

                if (enemies[i].TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(damage);
                }
            }
#if UNITY_EDITOR
            gizmoCenter = center;
            gizmoSize = boxData.size;
            gizmoRadius = boxData.radius;
            gizmoUseCircle = boxData.useCircle;
#endif
        }

        /// <summary>
        /// 持续检测伤害间隔判定
        /// </summary>
        private bool TryIntervalDamage(Collider2D other, int skillIndex)
        {
            float interval = 0f;
            if (skillIndex >= 0 && skillIndex < runtimeData.skillRuntimeData.Length)
                interval = runtimeData.skillRuntimeData[skillIndex].damageInterval;

            if (interval <= 0f) return continuousTargets.Add(other);

            if (intervalLastTime.TryGetValue(other, out float lastTime))
            {
                if (Time.time - lastTime < interval) return false;
            }
            intervalLastTime[other] = Time.time;

            return true;
        }

        private float GetDamage(int skillIndex)
        {
            if (runtimeData == null) return 0f;

            if (skillIndex >= 0 && skillIndex < runtimeData.skillRuntimeData.Length)
                return runtimeData.skillRuntimeData[skillIndex].damage;

            return runtimeData.damage;
        }

#if UNITY_EDITOR
        private Vector2 gizmoCenter;
        private Vector2 gizmoSize;
        private float gizmoRadius;
        private bool gizmoUseCircle;
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (gizmoUseCircle)
            {
                Gizmos.DrawWireSphere(gizmoCenter, gizmoRadius);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(gizmoCenter, Quaternion.identity, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
#endif
    }
}
