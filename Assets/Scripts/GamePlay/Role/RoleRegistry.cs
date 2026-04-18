using GamePlay.Role.RoleData;
using UnityEngine;

namespace GamePlay.Role
{
    [CreateAssetMenu(fileName = "RoleRegistry", menuName = "RoleData/RoleRegistry")]
    public class RoleRegistry : ScriptableObject
    {
        [System.Serializable]
        public struct RoleEntry
        {
            public RoleName roleName;
            public RoleBaseData template;
            public GameObject prefab;
        }
    
        public RoleEntry[] entries;
    
        public RoleEntry? GetEntry(RoleName roleName)
        {
            foreach (RoleEntry e in entries)
                if (e.roleName == roleName) return e;
            return null;
        }
    }
}