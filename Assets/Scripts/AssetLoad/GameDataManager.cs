using System;
using System.Threading.Tasks;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.ScriptObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AssetLoad
{
    public static class GameDataManager
    {
        public static RoleRegistry RoleRegistry { get; private set; }
        public static EquipmentPool EquipmentPool { get; private set; }
        public static ItemRarityScriptObject ItemRarityTable { get; private set; }

        /// <summary> 加载流程已结束（成功或失败都算结束），结果看 IsFailed </summary>
        public static bool IsReady { get; private set; }
        /// <summary> 加载流程已结束但存在失败项，消费者应据此降级 </summary>
        public static bool IsFailed { get; private set; }
        /// <summary> 正在加载中（用于需要轮询进度的场合） </summary>
        public static bool IsLoading { get; private set; }

        /// <summary> 加载流程结束时触发（成功与失败）</summary>
        public static event Action OnReady;

        
        public static void LoadResources()
        {
            if (IsReady || IsLoading) return;
            
            _ = LoadAsyncResource();
        }

        private static async Task LoadAsyncResource()
        {
            if (IsReady || IsLoading) return;
            IsLoading = true;

            try
            {
                RoleRegistry = await LoadAsync<RoleRegistry>(nameof(RoleRegistry));
                EquipmentPool = await LoadAsync<EquipmentPool>(nameof(EquipmentPool));
                ItemRarityTable = await LoadAsync<ItemRarityScriptObject>(nameof(ItemRarityTable));

                if (EquipmentPool != null) EquipmentPool.Initialize();

                IsFailed = RoleRegistry == null || EquipmentPool == null || ItemRarityTable == null;
            }
            catch (Exception e)
            {
                // 检查 LoadAssetAsync 的同步异常与 Initialize() 内部异常
                Debug.LogError($"[GameDataManager] 配置表加载抛出异常: {e}");
                IsFailed = true;
            }
            finally
            {
                IsLoading = false;
                IsReady = true;
                NotifyReady();
            }
        }
        
        private static void NotifyReady()
        {
            try
            {
                OnReady?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameDataManager] OnReady 订阅者抛出异常: {e}");
            }
        }

        private static async Task<T> LoadAsync<T>(string address) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load: {address}");
                return null;
            }

            return handle.Result;
        }
    }
}
