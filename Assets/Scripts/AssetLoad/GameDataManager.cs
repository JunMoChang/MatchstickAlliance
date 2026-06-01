using System;
using System.Threading.Tasks;
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

        public static bool IsReady { get; private set; }
        public static bool IsFailed { get; private set; }
        public static event Action OnReady;

        private static bool _isLoading;

        /// <summary>
        /// 自动在场景加载前启动异步加载
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit()
        {
            if (!IsReady && !_isLoading) InitAsync();
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        public static async void InitAsync()
        {
            if (IsReady || _isLoading) return;
            _isLoading = true;

            RoleRegistry = await LoadAsync<RoleRegistry>(nameof(RoleRegistry));
            EquipmentPool = await LoadAsync<EquipmentPool>(nameof(EquipmentPool));

            if (EquipmentPool != null)
                EquipmentPool.Initialize();

            IsFailed = (RoleRegistry == null || EquipmentPool == null);
            IsReady = true;
            _isLoading = false;

            OnReady?.Invoke();
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