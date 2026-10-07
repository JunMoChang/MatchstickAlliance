using System;
using System.Threading.Tasks;
using GamePlay.Role.RoleData.BaseData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
            public AssetReferenceGameObject prefab;

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

        /// <summary>
        /// 异步加载角色 prefab。加载失败或抛异常返回 null。
        /// 不要在调用方用 .Result / .Wait() / WaitForCompletion 取结果：那会把主线程锁在紧循环里，
        /// 与场景切换（Single 模式场景整合需要主线程）互相死锁。
        /// </summary>
        public static async Task<GameObject> LoadPrefab(RoleEntry entry)
        {
            if (entry.prefab == null || !entry.prefab.RuntimeKeyIsValid())
            {
                Debug.LogError($"[RoleRegistry] 角色 {entry.roleName} 的 prefab 引用未配置或无效"); 
                return null;
            }
            
            try
            {
                float startTime = Time.realtimeSinceStartup;
                AsyncOperationHandle<GameObject> handle = entry.prefab.LoadAssetAsync();
                Debug.Log($"[RoleRegistry] 开始加载角色 {entry.roleName}（key={entry.prefab.RuntimeKey}）");

                // Addressables 的 Task 在操作失败返回 null
                GameObject prefab = await handle.Task;

                Debug.Log($"[RoleRegistry] 角色 {entry.roleName} 加载结束：status={handle.Status}，耗时 {Time.realtimeSinceStartup - startTime:F2}s，结果={(prefab != null ? prefab.name : "null")}");

                if (prefab == null) Debug.LogError($"[RoleRegistry] 角色 {entry.roleName} 的 prefab 加载失败");
                
                return prefab;
            }
            catch (Exception e)
            {
                Debug.LogError($"[RoleRegistry] 角色 {entry.roleName} 的 prefab 加载异常: {e}");
                return null;
            }
        }

        /// <summary> 释放 LoadPrefab 加载的资源 </summary>
        public static void ReleasePrefab(RoleEntry entry)
        {
            if (entry.prefab == null || !entry.prefab.IsValid()) return;
            
            entry.prefab.ReleaseAsset();
        }
    }
}
