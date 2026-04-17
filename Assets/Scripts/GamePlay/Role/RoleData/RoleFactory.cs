using System.Collections.Generic;

namespace GamePlay.Role.RoleData
{
    public static class RoleFactory
    {
        
        private static Dictionary<RoleName, RoleSaveData> roleSaveData;
        
        public static RoleSaveData GetRoleSaveData(RoleName roleName)
        {
            return roleSaveData[roleName];
        }
        
        
    }
}