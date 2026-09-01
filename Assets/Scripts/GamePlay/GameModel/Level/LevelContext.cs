using System.Collections.Generic;
using GamePlay.Role.RoleData;

namespace GamePlay.GameModel.Level
{
    public static class LevelContext
    {
        public static ChapterData CurrentChapter;
        public static LevelData CurrentLevel;
        public static List<RoleRegistry.RoleEntry> SelectedHeroes;

        /// <summary> 是否处于 PvP 联机模式（默认 false = 单机） </summary>
        public static bool IsPvPMode;

        /// <summary> PvP 模式：本地玩家选中的角色 </summary>
        public static RoleRegistry.RoleEntry? PvPSelectedRole;

        /// <summary> 返回大厅请求：Main 场景加载完成后自动显示 PvPLobbyPanel </summary>
        public static bool ReturnToPvPLobby;

        /// <summary> 正在执行返回大厅流程（防重入，也用于区分主动关闭与意外断线） </summary>
        public static bool IsReturningToLobby;
    }
}
