namespace GamePlay.CharacterControllers.RoleControllers
{
    public interface IRoleStrategy
    {
        void Move(UnityEngine.Vector2 direction, UnityEngine.Rigidbody2D rb);
        void Attack();
        void UseSkill1();
        void UseSkill2();
        void UseSkill3();
        void UseSkill4();
        RoleData.BaseRole RoleData { get; }  
    }
}