using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "Scriptable Objects/ChapterData",  order = 0)]
    public class ChapterData :  ScriptableObject
    {
        public int chapter;
        public string sceneName;
        public LevelData[] levelData;
        public LevelDrapConfig dorpConfig;
        
        [SerializeField] private int baseExperience;
        [SerializeField] private float experienceGrowthToLevel;
        [SerializeField] private int baseGold;
        [Min(1)]
        [SerializeField] private float goldGrowthToLevel;
        [SerializeField] private int diamondPerLevel;
        [SerializeField] private int diamondFirstLevel;
        public int DiamondsPerLevel => diamondPerLevel;
        public int DiamondsFirstLevel => diamondFirstLevel;

        public int GetGold(int levelIndex)
        {
            return Mathf.FloorToInt(baseGold * Mathf.Pow(goldGrowthToLevel, levelIndex));
        }

        public int GetExp(int levelIndex)
        {
            return Mathf.FloorToInt(baseExperience * Mathf.Pow(experienceGrowthToLevel, levelIndex));
        }
    }
}