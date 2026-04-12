using GamePlay.CharacterControllers.RoleControllers;
using UnityEngine;

namespace GamePlay.CharacterControllers.RoleData
{
    public class YasuoStrategy : BaseRole, IRoleStrategy
    {
        public override Sprite Skill1 { get; protected set; }
        public override Sprite Skill2 { get; protected set; }
        public override Sprite Skill3 { get; protected set; }
        public override Sprite Skill4 { get; protected set; }
        public void Move(Vector2 direction, Rigidbody2D rb)
        {
            throw new System.NotImplementedException();
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