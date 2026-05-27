using GamePlay.EnemyConfiguration;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData",  order = 1)]
    public class LevelData : ScriptableObject
    {
        public int levelIndex;
        public LevelSprite sprite;
        public WaveData[] waves;
        
        public int GetTotalEnemies()
        {
            int totalEnemies = 0;
            foreach (WaveData waveData in waves)
            {
                foreach (LevelEnemyConfiguration enemyConfig in waveData.enemies)
                {
                    totalEnemies += enemyConfig.spawnCount;
                }
            }
            return totalEnemies;
        }
    }
}
