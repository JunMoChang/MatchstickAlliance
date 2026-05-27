using System.Collections.Generic;
using GamePlay.Role.RoleData;

namespace GamePlay.GameModel.Level
{
    public static class LevelContext
    {
        public static ChapterData CurrentChapter;
        public static LevelData CurrentLevel;
        public static List<RoleRegistry.RoleEntry> SelectedHeroes;
    }
}
