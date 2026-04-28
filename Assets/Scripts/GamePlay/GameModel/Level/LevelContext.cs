using System.Collections.Generic;
using GamePlay.Role;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelContext", menuName = "Scriptable Objects/LevelContext")]
    public class LevelContext : ScriptableObject
    {
        public ChapterData currentChapter;
        public LevelData currentLevel;
        public List<RoleRegistry.RoleEntry> selectedHeroes;
    }
}
