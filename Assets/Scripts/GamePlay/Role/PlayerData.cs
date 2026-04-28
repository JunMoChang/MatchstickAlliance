using System;
using System.Collections.Generic;
using GamePlay.Role.RoleData;
using Newtonsoft.Json;

namespace GamePlay.Role
{
    public class PlayerData
    {
        
        /// <summary>
        /// 解锁的角色
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<RoleName, RoleSaveData> ownedRoles = new ()
        {
            { RoleName.孙悟空,  new RoleSaveData{ roleName = RoleName.孙悟空, roleLevel = 1} }
        };
        
        /// <summary>
        /// 游戏货币数量
        /// </summary>
        public class GameProps
        {
            public int coins;
            public int diamonds;
        }
        
        /// <summary>
        /// 以完成的章节数
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
            DateTimeOffset.FromUnixTimeSeconds(saveTimestamp)
                .ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm:ss");
    }
}