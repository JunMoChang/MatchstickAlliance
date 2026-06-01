using System;
using GamePlay.Role.RoleData.BaseData;
using UnityEngine;

namespace GamePlay.Role.RoleData
{
    [CreateAssetMenu(fileName = "RoleRegistry", menuName = "RoleData/RoleRegistry")]
    public class RoleRegistry : ScriptableObject
    {
        [Serializable]
        public struct RoleEntry : IEquatable<RoleEntry>
        {
            public RoleName roleName;
            public RoleBaseData template;
            public GameObject prefab;

            public bool Equals(RoleEntry other)
            {
                return roleName == other.roleName;
            }

            public override bool Equals(object obj)
            {
                return obj is RoleEntry other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (int)roleName;
            }
        }
    
        public RoleEntry[] entries;

        public RoleEntry? GetRoleEntry(RoleName roleName)
        {
            foreach (RoleEntry e in entries)
            {
                if (e.roleName == roleName) return e;
            }
            return null;
        }
    }
}