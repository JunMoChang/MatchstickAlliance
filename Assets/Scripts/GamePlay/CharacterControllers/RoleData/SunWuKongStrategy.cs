using GamePlay.CharacterControllers.RoleControllers;
using UnityEngine;

namespace GamePlay.CharacterControllers.RoleData
{
    [System.Serializable]
    public class SunWuKongStrategy : BaseRole, IRoleStrategy
    {
        public override Sprite Skill1 { get; protected set; }
        public override Sprite Skill2 { get; protected set; }
        public override Sprite Skill3 { get; protected set; }
        public override Sprite Skill4 { get; protected set; }
        public SunWuKongStrategy(string name, float defaultHealth, float defaultDamage) : base(name, defaultHealth, defaultDamage)
        {
            
        }


        public void Move(Vector2 direction, Rigidbody2D rb)
        {
            rb.MovePosition(direction * speed);
        }

        public void Attack()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill1()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill2()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill3()
        {
            throw new System.NotImplementedException();
        }

        public void UseSkill4()
        {
            throw new System.NotImplementedException();
        }

        public BaseRole RoleData { get; }
    }
}