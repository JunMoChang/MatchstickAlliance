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
            { RoleName.孙悟空, new RoleSaveData { roleName = RoleName.孙悟空, roleLevel = 1 } }
        };
        
        /// <summary>
        /// 游戏货币
        /// </summary>
        [JsonProperty]
        public readonly GameCurrency gameProps = new();

        public readonly List<ItemInstance> inventoryItems = new();
        /// <summary>
        /// 已装备的物品名称列表
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<ItemInstance> equippedItems = new();

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
