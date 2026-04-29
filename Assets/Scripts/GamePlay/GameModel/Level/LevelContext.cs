using System.Collections.Generic;
using GamePlay.Role;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    public class LevelContext : ScriptableObject
    {
        private static LevelContext instance;
        public static LevelContext Instance
        {
            get
            {
                if (instance == null)
                    instance = CreateInstance<LevelContext>();
                return instance;
            }
        }

        public ChapterData currentChapter;
        public LevelData currentLevel;
        public List<RoleRegistry.RoleEntry> selectedHeroes;
    }
}
