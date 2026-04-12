using GamePlay.EnemyConfiguration;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelContext levelContext;

        
        private void InitializeLevel()
        {
            Debug.Log($"开始游戏：第{levelContext.currentChapter.chapter}章，关卡 {levelContext.currentLevel.levelIndex}");
        }
        
        public static class LevelScaler
        {
            public static float GetHp(EnemyData data, int levelChapter)
            {
                return data.baseHp * Mathf.Pow(data.hpGrowthRate, levelChapter);
            }

            public static float GetDamage(EnemyData data, int levelChapter)
            {
                return data.baseDamage * Mathf.Pow(data.damageGrowthRate, levelChapter);
            }
        }
    }
}