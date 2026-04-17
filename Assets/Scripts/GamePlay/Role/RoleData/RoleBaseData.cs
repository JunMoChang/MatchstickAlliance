using UnityEngine;

namespace GamePlay.Role.RoleData
{
    [System.Serializable]
    public abstract class RoleBaseData : ScriptableObject
    {
        /// <summary>
        /// 瞬间攻击碰撞检测数据
        /// </summary>
        [System.Serializable]
        public struct InstantHitData
        {
            public RoleBoxCollider.BoxColliderManager.InstantBoxColliderName instantBoxName;
            public Vector2 offset;
            public Vector2 size;
            public float radius;
            public bool useCircle;
            public InstantHitData(RoleBoxCollider.BoxColliderManager.InstantBoxColliderName _name, Vector2 _offset, Vector2 _size, float _radius, bool _useCircle)
            {
                instantBoxName = _name;
                offset = _offset;
                size = _size;
                radius = _radius;
                useCircle = _useCircle;
            }
        }
        
        [System.Serializable]
        public struct MotionCommand
        {
            public MotionName motionName;
            public MotionData[] motionData;
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
            Parabolic
            
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
            }
        }
        
        public string roleName;
        public float defaultHealth = 100f;
        public float defaultSpeed = 3.5f;
        public float defaultDamage = 10f;
        public float healthGrowth;
        public float damageGrowth;
        
        [SerializeField] private Sprite skill1;
        [SerializeField] private Sprite skill2;
        [SerializeField] private Sprite skill3;
        [SerializeField] private Sprite skill4;
    
        public Sprite Skill1 => skill1;
        public Sprite Skill2 => skill2;
        public Sprite Skill3 => skill3;
        public Sprite Skill4 => skill4;
    }
}