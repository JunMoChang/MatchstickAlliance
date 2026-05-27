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
        private int currentWaveIndex;
        private bool waveInProgress;
        private int remainingEnemies;
        
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
            WaveData[] waves = LevelContext.CurrentLevel.waves;

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
            while (remainingEnemies > 0) yield return null;
        }
        
        private void SpawnWave(WaveData wave)
        {
            int chapter = LevelContext.CurrentChapter.chapter;
            
            foreach (LevelEnemyConfiguration cfg in wave.enemies)
            {
                remainingEnemies += cfg.spawnCount;
                
                float factor = cfg.GetGrowthFactor(chapter);
                float hp = LevelManager.LevelScaler.GetHp(cfg.enemyData, factor);
                float damage = LevelManager.LevelScaler.GetDamage(cfg.enemyData, factor); 

                for (int i = 0; i < cfg.spawnCount; i++)
                {
                    Vector2 pos = new(player.position.x + Random.Range(-maxOffset, maxOffset), player.position.y);
                    GameObject enemy = Instantiate(cfg.enemyData.enemyPrefab, pos, Quaternion.identity);
                    EnemyContext context = enemy.GetComponent<EnemyContext>();
                    context.Init(hp, damage, player, cfg.enemyData);
                    context.OnEnemyDied += OnEnemyDied;
                }
            }
        }
        
        private void OnEnemyDied(Vector2 pos)
        {
            remainingEnemies--;
            DropManager.Instance.TryDrop(pos);
        }
        
        private void ClearUp()
        {
            StopAllCoroutines();
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