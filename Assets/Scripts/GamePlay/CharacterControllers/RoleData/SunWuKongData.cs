using UnityEngine;

namespace GamePlay.CharacterControllers.RoleData
{
    public class SunWuKongData : RoleBaseData
    {
        public override Sprite Skill1 { get; protected set; }
        public override Sprite Skill2 { get; protected set; }
        public override Sprite Skill3 { get; protected set; }
        public override Sprite Skill4 { get; protected set; }
        
        public SunWuKongData(string name, float defaultHealth, float defaultDamage) : base(name, defaultHealth, defaultDamage)
        {
        }
    }
}