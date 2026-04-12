using UnityEngine;

namespace GamePlay.CharacterControllers.RoleData
{
    [System.Serializable]
    public abstract class BaseRole
    {
        public string name;
        public float defaultHealth = 100f;
        public float defaultSpeed = 2f;
        public float defaultDamage = 10f;
        public float speed = 3.5f;
        public abstract Sprite Skill1 {get; protected set;}
        public abstract Sprite Skill2 {get; protected set;}
        public abstract Sprite Skill3 {get; protected set;}
        public abstract Sprite Skill4 {get; protected set;}
        
        protected BaseRole()
        {
            
        }
        protected BaseRole(string name, float defaultHealth, float defaultDamage)
        {
            this.name = name;
            this.defaultHealth = defaultHealth;
            this.defaultDamage = defaultDamage;
        }
    }
}