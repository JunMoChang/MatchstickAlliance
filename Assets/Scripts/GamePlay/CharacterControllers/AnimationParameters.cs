using UnityEngine;

namespace CharacterControllers
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
    }
}