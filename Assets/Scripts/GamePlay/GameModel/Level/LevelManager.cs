using System;
using GamePlay.EnemyConfiguration;
using GamePlay.Scene;
using GamePlay.UI;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    public class LevelManager : MonoBehaviour
    {
        public LevelManager Instance { get; private set; }
        
        public static Action<Action> OnLevelEnded;

        private bool levelEndInProgress;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

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
            Debug.Log("LevelEnd");
            OnLevelEnded?.Invoke(OnRewardFinished);
        }
        private void OnRewardFinished()
        {
            UIManager.ShowLevelEndedPanel();
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