using System;
using GamePlay.Role.RoleBoxCollider;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Role.RoleData.BaseData
{
    [Serializable]
    public abstract class RoleBaseData : ScriptableObject
    {
        #region 角色基本属性

        public RoleName roleName;
        public int roleLevel;
        public int defaultStar = 3;
        
        public float defaultHealth = 100f;
        [Range(1f, 2f)] public float healthGrowth;
        
        public float defaultDefense;
        [Range(1f, 2f)] public float defenseGrowth;
        
        public float defaultDamage = 10f;
        [Range(1f, 2f)] public float damageGrowth;
        
        public float defaultCritRate = 0.1f;
        
        public float defaultSpeed = 3.5f;
        
        public SkillData[] skillsBaseData;
        public int[] skillDamageGrowth;
        
        public Sprite exhibitionIcon;
        public Sprite unSelectedIcon;
        public Sprite selectedIcon;
        
        public int lockPrice;

        #endregion
        
        /// <summary> 技能数据 </summary>
        [Serializable]
        public struct SkillData
        {
            /// <summary> 技能图标 </summary>
            public Sprite icon;
            /// <summary> 技能等级 </summary>
            public int level;   
            /// <summary> 基础攻击 </summary>
            public float baseDamage;
            /// <summary> 伤害 </summary>
            public int damage;
            /// <summary> 基础冷却 </summary>
            public float baseCooldown;
            /// <summary> 伤害间隔（秒）,0 = 仅进入时伤害一次 </summary>
            public float damageInterval;
            /// <summary> 释放期间是否允许自由移动（释放中不冻结输入移动） </summary>
            public bool canMoveWhileCasting;
            /// <summary> 自由移动允许的结束动画进度 </summary>
            [Range(0f, 1f)] public float freeMoveEndProgress;
        }
        
        /// <summary> 攻击盒几何数据 </summary>
        [Serializable]
        public struct HitBoxData
        {
            public Vector2 offset;
            public Vector2 size;
            public float radius;
            public bool useCircle;
            [Tooltip("技能索引，普攻或无需技能伤害时设为 -1，将使用默认伤害")]
            public int skillIndex;
        }
        
        /// <summary> 行为名称 </summary>
        public enum ActionName
        {
            Normal_1, Normal_2, Normal_3, Normal_4, Normal_5,
            Skill_1, Skill_2, Skill_3, Skill_4
        }
        
        /// <summary> 运动触发命令 </summary>
        [Serializable]
        public struct MotionCommand
        {
            public ActionName actionName;
            public MotionKeyframe[] keyframes;
        }
        /// <summary> 运动的触发时机 </summary>
        [Serializable]
        public struct MotionKeyframe
        {
            /// <summary> 动画进度 </summary>
            [Range(0f, 1f)]
            public float normalizedTime;
            /// <summary> 运动数据 </summary>
            public MotionData motionData;
        }
        /// <summary> 运动数据 </summary>
        [Serializable]
        public struct MotionData
        {
            /// <summary>运动控制类型 </summary>
            public MotionType motionType;
            /// <summary> 设置速度 </summary>
            public Vector2 velocity;
            /// <summary> 设置位移量</summary>
            public Vector2 offset;
            /// <summary> 设置冲力 </summary>
            public Vector2 force;
            /// <summary> 设置重力 </summary>
            public float gravityScale;
            
            /// <summary> 运动控制类型 </summary>
            public enum MotionType
            {
                /// <summary> 速度控制 </summary>
                LinearVelocity,
                /// <summary> 位置控制 </summary>
                MovePosition,
                /// <summary> 冲力控制 </summary>
                AddForce,
                /// <summary> 重力控制 </summary>
                GravityScale,
                /// <summary> 清除控制 </summary>
                ClearVelocity
            }
        }
        
        /// <summary> 碰撞盒触发命令 </summary>
        [Serializable]
        public struct HitBoxCommand
        {
            public ActionName actionName;
            public HitBoxKeyframe[] keyframes;
        }
        /// <summary> 碰撞盒触发时机 </summary>
        [Serializable]
        public struct HitBoxKeyframe
        {
            /// <summary> 动画进度 </summary>
            [Range(0f, 1f)]
            public float normalizedTime;
            /// <summary> 触发动作 </summary>
            public HitBoxAction action;
            /// <summary> 攻击盒逻辑标识 </summary>
            public BoxColliderManager.HitBoxName boxName;
            /// <summary> 瞬时盒伤害检测窗口时长 </summary>
            public float windowSeconds;
            /// <summary> 攻击盒几何配置 </summary>
            [FormerlySerializedAs("data")] public HitBoxData boxData;
        }

        /// <summary> 碰撞盒触发动作类型 </summary>
        public enum HitBoxAction
        {
            EnableBox,
            DisableBox,
            InstantHit
        }

        
        public void FirstLoadSaveData(RoleSaveData saveData)
        {
            saveData.roleLevel = roleLevel;
            saveData.baseAttributes = new BaseAttributes
            {
                health = defaultHealth,
                damage = defaultDamage,
                defense = defaultDefense,
                critRate = defaultCritRate
            };
            saveData.speed = defaultSpeed;

            int length = skillsBaseData.Length;
            saveData.skillsData = new SkillSaveData[length];
            for (int i = 0; i < length; i++)
            {
                saveData.skillsData[i] = new SkillSaveData
                {
                    level = skillsBaseData[i].level,
                    damage = skillsBaseData[i].damage
                };
            }
        }
        
        /// <summary>
        /// 更新角色属性
        /// </summary>
        /// <param name="saveData">持久化数据类</param>
        /// <param name="enhance">等级提升数量</param>
        public void UpdateSaveData(RoleSaveData saveData, int enhance)
        {
            saveData.roleLevel += enhance;
            saveData.baseAttributes = new BaseAttributes
            {
                health = defaultHealth * healthGrowth * enhance,
                defense = defaultDefense * defenseGrowth * enhance,
                damage = defaultDamage * damageGrowth * enhance,
            };
            saveData.speed = defaultSpeed;
        }
        
        /// <summary>
        /// 更新角色技能属性
        /// </summary>
        /// <param name="saveData">持久化数据类</param>
        /// <param name="skillIndex">目标技能</param>
        /// <param name="enhance">等级提升数量</param>
        public void UpdateSkillData(RoleSaveData saveData, int skillIndex, int enhance)
        {
            saveData.skillsData ??= new SkillSaveData[skillsBaseData.Length];

            saveData.skillsData[skillIndex].level += enhance;
            saveData.skillsData[skillIndex].damage += skillDamageGrowth[skillIndex] * enhance;
        }
    } 
}