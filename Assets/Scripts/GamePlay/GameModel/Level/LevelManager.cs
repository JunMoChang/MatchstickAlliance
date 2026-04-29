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
            public static float GetHp(EnemyData data, float levelFactor)
            {
                return data.baseHp * Mathf.Pow(data.hpGrowthRate, levelFactor);
            }

            public static float GetDamage(EnemyData data, float levelFactor)
            {
                return data.baseDamage * Mathf.Pow(data.damageGrowthRate, levelFactor);
            }
        }

        private void OnDisable()
        {
            SceneLoader.OnLevelLoaded -= FindObject;
        }
    }
}