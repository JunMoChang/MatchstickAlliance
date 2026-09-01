using UnityEngine;

namespace GamePlay.GameModel
{
    public static class AnimationParameters
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int NormalCombos = Animator.StringToHash("NormalCombos");
        public static readonly int Skill_1 = Animator.StringToHash("Skill_1");
        public static readonly int Skill_2 = Animator.StringToHash("Skill_2");
        public static readonly int Skill_3 = Animator.StringToHash("Skill_3");
        public static readonly int Skill_4 = Animator.StringToHash("Skill_4");
        public static readonly int Attacked_Trigger = Animator.StringToHash("Attacked");
        public static readonly int Death_Bool = Animator.StringToHash("Death");
        
        public static readonly int[] SkillTriggers = { Skill_1, Skill_2, Skill_3, Skill_4 };
        
        // 敌人动画参数
        public static readonly int EnemySpeed = Animator.StringToHash("Speed");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Hit = Animator.StringToHash("Hit");
        public static readonly int Dead = Animator.StringToHash("Dead");
    }
}