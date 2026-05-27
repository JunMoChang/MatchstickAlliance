using System;
using GamePlay.Role.RoleBoxCollider;
using UnityEngine;

namespace GamePlay.Role.RoleData
{
    [Serializable]
    public abstract class RoleBaseData : ScriptableObject
    {
        public RoleName roleName;
        public int roleLevel;
        public int defaultStar = 1;
        public float defaultHealth = 100f;
        public float defaultSpeed = 3.5f;
        public float defaultDamage = 10f;
        public float healthGrowth;
        public float damageGrowth;
        public float[] skillDamageGrowth;
        public SkillData[] skills;
        public Sprite unSelectedIcon;
        public Sprite selectedIcon;
        /// <summary>
        /// 技能数据
        /// </summary>
        [Serializable]
        public struct SkillData
        {
            /// <summary>
            /// 技能图标
            /// </summary>
            public Sprite icon;
            /// <summary>
            /// 技能等级
            /// </summary>
            public int level;
            /// <summary>
            /// 基础伤害
            /// </summary>
            public float baseDamage;
            /// <summary>
            /// 基础冷却
            /// </summary>
            public float baseCooldown;
            /// <summary>
            /// 伤害间隔（秒）,0 = 仅进入时伤害一次
            /// </summary>
            public float damageInterval;
        }
        
        /// <summary>
        /// 持续攻击碰撞检测数据
        /// </summary>
        [Serializable]
        public struct ContinuousHitBoxData
        {
            public BoxColliderManager.BoxColliderName boxColliderName;
            public Collider2D collider;
            [Tooltip("技能索引，-1 使用默认伤害")]
            public int skillIndex;
        }
        
        /// <summary>
        /// 瞬间攻击碰撞检测数据
        /// </summary>
        [Serializable]
        public struct InstantHitBoxData
        {
            public BoxColliderManager.InstantBoxColliderName instantBoxName;
            public Vector2 offset;
            public Vector2 size;
            public float radius;
            public bool useCircle;
            [Tooltip("技能索引，普攻或无需技能伤害时设为 -1，将使用默认伤害")]
            public int skillIndex;
        }
        
        [Serializable]
        public struct MotionCommand
        {
            public MotionName motionName;
            public MotionKeyframe[] keyframes;
        }
        
         /// <summary>
        /// 运动标识名称
        /// </summary>
        public enum MotionName
        {
            Skill_1,
            Skill_2,
            Skill_3,
            Skill_4,
            Normal_1,
            Normal_2,
            Normal_3,
            Normal_4,
            Normal_5,
            SkillEnd
        }
         
        /// <summary>
        /// 运动的触发时机
        /// </summary>
        [Serializable]
        public struct MotionKeyframe
        {
            /// <summary>
            /// 动画进度
            /// </summary>
            [Range(0f, 1f)]
            public float normalizedTime;
            /// <summary>
            /// 运动数据
            /// </summary>
            public MotionData motionData;
        }
        
        /// <summary>
        /// 运动数据
        /// </summary>
        [System.Serializable]
        public struct MotionData
        {
            /// <summary>
            /// 运动控制类型
            /// </summary>
            public MotionType motionType;
            /// <summary>
            /// 设置速度
            /// </summary>
            public Vector2 velocity;
            /// <summary>
            /// 设置位移量
            /// </summary>
            public Vector2 offset;
            /// <summary>
            /// 设置冲力
            /// </summary>
            public Vector2 force;
            /// <summary>
            /// 设置重力
            /// </summary>
            public float gravityScale;
            public bool playerControlledDirection;
            
            /// <summary>
            /// 运动控制类型
            /// </summary>
            public enum MotionType
            {
                /// <summary>
                /// 速度控制
                /// </summary>
                LinearVelocity,
                /// <summary>
                /// 位置控制
                /// </summary>
                MovePosition,
                /// <summary>
                /// 冲力控制
                /// </summary>
                AddForce,
                /// <summary>
                /// 重力控制
                /// </summary>
                GravityScale,
                /// <summary>
                /// 清除控制
                /// </summary>
                ClearVelocity
            }
        }

        public void FirstLoadSaveData(RoleSaveData save)
        {
            save.maxHealth = defaultHealth;
            save.damage = defaultDamage;
            save.speed = defaultSpeed;
        
            int length = skills.Length;
            save.skillsLevel = new int[length];
            save.skillsDamages = new float[length];
            save.skillsCooldowns = new float[length];
            save.damageIntervals = new float[length];
            for (int i = 0; i < length; i++)
            {
                save.skillsDamages[i] = skills[i].baseDamage;
                save.skillsCooldowns[i] = skills[i].baseCooldown;
                save.damageIntervals[i] = skills[i].damageInterval;
            }
        }
        /// <summary>
        /// 更新角色属性
        /// </summary>
        /// <param name="save">持久化数据类</param>
        /// <param name="enhance">等级提升数量</param>
        public void RefreshSaveData(RoleSaveData save, int enhance)
        {
            save.roleLevel += enhance;
            save.maxHealth = defaultHealth * (1 + healthGrowth) * enhance;
            save.damage = defaultDamage * (1 + damageGrowth) *  enhance;
            save.speed = defaultSpeed;
        }
        /// <summary>
        /// 更新角色技能属性
        /// </summary>
        /// <param name="data">持久化数据类</param>
        /// <param name="skillIndex">目标技能</param>
        /// <param name="enhance">等级提升数量</param>
        public void RefreshSkillsSaveData(RoleSaveData data, int skillIndex, int enhance)
        {
            if (data.skillsCooldowns == null || data.skillsCooldowns.Length < skills.Length) data.skillsCooldowns = new float[skills.Length];
            if (data.skillsDamages == null || data.skillsDamages.Length < skills.Length) data.skillsDamages = new float[skills.Length];
            if (data.damageIntervals == null || data.damageIntervals.Length < skills.Length) data.damageIntervals = new float[skills.Length];

            data.skillsLevel[skillIndex] += enhance;
            data.skillsDamages[skillIndex] = skills[skillIndex].baseDamage * (1 + damageGrowth) * (1 + (data.skillsLevel[skillIndex] - 1) * 0.15f);
            data.skillsCooldowns[skillIndex] = skills[skillIndex].baseCooldown;
            data.damageIntervals[skillIndex] = skills[skillIndex].damageInterval;
        }
    }

    
}