using System.Collections.Generic;
using GamePlay.Role.RoleData;

namespace GamePlay.Role
{
    public class PlayerData
    {
        public List<RoleSaveData> ownedRoles = new(); //解锁的角色

        public class GameProps
        {
            public int coins;
            public int diamonds;
        }
        
        public int level; //当前章节已完成的关卡数
        public int levelChapter; //以为完成的章节数
        
        public long saveTimestamp; //存档时间
    }
}