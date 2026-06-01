using System;
using System.Collections.Generic;
using GamePlay.Inventory.ScriptObjects;
using GamePlay.Role;
using GamePlay.Role.RoleData;
using Newtonsoft.Json;

namespace GamePlay.PlayerDataHandle
{
    public class PlayerData
    {
        /// <summary>
        /// 解锁的角色
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public readonly Dictionary<RoleName, RoleSaveData> ownedRoles = new()
        {
            { RoleName.猴子, new RoleSaveData { roleName = RoleName.猴子, roleLevel = 1 } }
        };
        
        /// <summary>
        /// 游戏货币
        /// </summary>
        [JsonProperty]
        public readonly GameCurrency gameProps = new();

        public readonly List<ItemInstance> inventoryItems = new();
        /// <summary>
        /// 各角色已装备的物品（角色名 → 装备列表）
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<RoleName, List<ItemInstance>> roleEquippedItems = new();

        /// <summary>
        /// 获取指定角色的已装备物品列表
        /// </summary>
        public List<ItemInstance> GetEquippedItemsForRole(RoleName role)
        {
            roleEquippedItems.TryGetValue(role, out List<ItemInstance> items);
            return items ?? new List<ItemInstance>();
        }

        /// <summary>
        /// 设置指定角色的已装备物品
        /// </summary>
        public void SetEquippedItemsForRole(RoleName role, List<ItemInstance> items)
        {
            if (items == null || items.Count == 0)
                roleEquippedItems.Remove(role);
            else
                roleEquippedItems[role] = items;
        }

        /// <summary>
        /// 已首次通关的关卡ID（chapter * 1000 + levelIndex）
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public readonly HashSet<int> firstClearedLevelIds = new();

        /// <summary>
        /// 已完成的章节数
        /// </summary>
        public int levelChapter;
        /// <summary>
        /// 当前章节已完成的关卡数
        /// </summary>
        public int level;
        /// <summary>
        /// 存档时间
        /// </summary>
        public long saveTimestamp;

        [JsonProperty("saveTime")]
        public string SaveTimeReadable =>
            DateTimeOffset.FromUnixTimeSeconds(saveTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

        public class ItemInstance
        {
            /// <summary>
            /// 物品唯一标识，用于存档加载时精确匹配装备归属
            /// </summary>
            [JsonProperty]
            public string instanceId = Guid.NewGuid().ToString("N");
            public ItemScriptableObject.ItemName itemName;
            public ItemRarityScriptObject.ItemRarity itemRarity;
            public int owenQuantities;
            public int level;
        }
        
        public class GameCurrency
        {
            public int gold;
            public int diamonds;
        }
    }
}
