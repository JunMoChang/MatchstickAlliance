using System;
using GamePlay.EnemyConfiguration;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static event Action<Action> OnLevelEnded;
        public static event Action OnRewardCompleted;

        private bool levelEndInProgress;
        private ChapterData currentChapter;
        private LevelData currentLevel;
        void Awake()
        {
            SceneLoader.OnLevelLoaded += FindObject;
        }
        private void FindObject()
        {
            levelEndInProgress = false;
            EnemyManager enemyManager = FindAnyObjectByType<EnemyManager>();
            if(enemyManager != null)
                enemyManager.OnAllWavesCleared += TriggerLevelEnd;
        }
        private void TriggerLevelEnd()
        {
            if (levelEndInProgress) return;
            levelEndInProgress = true;
            OnLevelEnded?.Invoke(OnRewardFinished);
        }
        private void OnRewardFinished()
        {
            OnRewardCompleted?.Invoke();
        }
        public static class LevelScaler
        {
            /// <summary>
            /// 敌人属性随章节成长，每章按 (1+rate) 累乘。
            /// </summary>
            public static float GetHp(EnemyData data, float levelFactor)
            {
                return data.baseHp * Mathf.Pow(1f + data.hpGrowthRate, levelFactor - 1f);
            }

            public static float GetDamage(EnemyData data, float levelFactor)
            {
                return data.baseDamage * Mathf.Pow(1f + data.damageGrowthRate, levelFactor - 1f);
            }
        }

        private void OnDisable()
        {
            SceneLoader.OnLevelLoaded -= FindObject;
        }
    }
}