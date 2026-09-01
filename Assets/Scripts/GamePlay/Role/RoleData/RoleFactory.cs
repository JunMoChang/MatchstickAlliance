using GamePlay.Role.RoleStrategy;

namespace GamePlay.Role.RoleData
{
    public static class RoleFactory
    {
        public static IRoleStrategy CreateRoleStrategy(RoleName roleName, RoleContext context)
        {
            IRoleStrategy strategy = roleName switch
            {
                RoleName.猴子 => new MonkeyStrategy(),
                RoleName.武士 => new WarriorStrategy(),
                _ => throw new System.Exception($"未注册的角色: {roleName}")
            };
            
            strategy.Initialize(context);
            return strategy;
        }
    }
}