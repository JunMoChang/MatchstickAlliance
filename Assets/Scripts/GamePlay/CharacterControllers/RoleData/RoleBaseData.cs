using UnityEngine;

namespace GamePlay.CharacterControllers.RoleData
{
    [System.Serializable]
    public abstract class RoleBaseData
    {
        public string name;
        public float defaultHealth = 100f;
        public float defaultSpeed = 3.5f;
        public float defaultDamage = 10f;

        public abstract Sprite Skill1 {get; protected set;}
        public abstract Sprite Skill2 {get; protected set;}
        public abstract Sprite Skill3 {get; protected set;}
        public abstract Sprite Skill4 {get; protected set;}
        
        protected RoleBaseData()
        {
            
        }
        protected RoleBaseData(string name, float defaultHealth, float defaultDamage)
        {
            this.name = name;
            this.defaultHealth = defaultHealth;
            this.defaultDamage = defaultDamage;
        }
    }
}