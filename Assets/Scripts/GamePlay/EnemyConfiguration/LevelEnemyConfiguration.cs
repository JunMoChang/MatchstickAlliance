using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
    [System.Serializable]
    public class LevelEnemyConfiguration
    {
        public EnemyData enemyData; //基础数据
        [SerializeField] private float chapterOverride; // 覆盖章节系数，0表示用LevelData.chapter
        public int spawnCount;

        public float GetGrowthFactor(int chapter)
        {
            return chapter + chapterOverride;
        }
    }
}