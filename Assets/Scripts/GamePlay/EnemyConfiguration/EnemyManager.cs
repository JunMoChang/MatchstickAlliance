using System;
using System.Collections;
using System.Collections.Generic;
using GamePlay.GameModel.Level;
using GamePlay.Scene;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.EnemyConfiguration
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private float maxOffset;
        private Transform player;
        
        private Transform[] createPosition;
        private List<GameObject> activeEnemies  = new (10);
        private int currentWaveIndex;
        private bool waveInProgress;
        
        public event Action OnAllWavesCleared;
        void Awake()
        {
            SceneLoader.OnLevelExit += ClearUp;
            SceneLoader.OnLevelLoaded += StartWaveSequence;
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        private void StartWaveSequence()
        {
            currentWaveIndex = 0;
            waveInProgress = false;
            StartCoroutine(WaveSequenceLoop());
        }
        
        private IEnumerator WaveSequenceLoop()
        {
            WaveData[] waves = LevelContext.Instance.currentLevel.waves;

            while (currentWaveIndex < waves.Length)
            {
                WaveData wave = waves[currentWaveIndex];
                
                yield return WaitForPlayerTrigger(wave.triggerX);
                
                SpawnWave(wave);
                waveInProgress = true;
                
                yield return WaitForWaveClear();

                waveInProgress = false;
                currentWaveIndex++;
            }
            
            OnAllWavesCleared?.Invoke();
        }
        
        private IEnumerator WaitForPlayerTrigger(float triggerX)
        {
            while (player.position.x <= triggerX) yield return null;
        }

        private IEnumerator WaitForWaveClear()
        {
            while (true)
            {
                bool allDead = true;
                foreach (GameObject enemy in activeEnemies)
                {
                    if (enemy != null) { allDead = false; break; }
                }
                if (allDead) break;
                yield return null;
            }
            activeEnemies.Clear();
        }
        
        private void SpawnWave(WaveData wave)
        {
            int chapter = LevelContext.Instance.currentChapter.chapter;

            foreach (LevelEnemyConfiguration cfg in wave.enemies)
            {
                float factor = cfg.GetGrowthFactor(chapter);
                float hp = LevelManager.LevelScaler.GetHp(cfg.enemyData, factor);
                float damage = LevelManager.LevelScaler.GetDamage(cfg.enemyData, factor);

                for (int i = 0; i < cfg.spawnCount; i++)
                {
                    Vector2 pos = new(player.position.x + Random.Range(-maxOffset, maxOffset), player.position.y);
                    GameObject enemy = Instantiate(cfg.enemyData.enemyPrefab, pos, Quaternion.identity);
                    enemy.GetComponentInChildren<EnemyContext>().Init(hp, damage);
                    activeEnemies.Add(enemy);
                }
            }
        }
        
        private void ClearUp()
        {
            StopAllCoroutines();
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            activeEnemies.Clear();
            waveInProgress = false;
            currentWaveIndex = 0;
        }

        void OnDisable()
        {
            SceneLoader.OnLevelExit -= ClearUp;
            SceneLoader.OnLevelLoaded -= StartWaveSequence;
        }
    }
}