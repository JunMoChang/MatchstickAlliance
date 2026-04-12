namespace GamePlay.EnemyConfiguration
{
    [System.Serializable]
    public class LevelEnemyConfiguration
    {
        public EnemyData enemyData; //基础数据
        public int chapterOverride; // 覆盖章节系数，0表示用LevelData.chapter
        public int spawnCount;
    }
}