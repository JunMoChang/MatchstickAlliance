using System;
using System.Collections;
using GamePlay.GameModel.Level;
using GamePlay.Scene;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.EnemyConfiguration
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private float maxOffset;
        [SerializeField] private float spawnInterval = 0.3f;
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
                
                yield return SpawnWave(wave);
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
        
        private IEnumerator SpawnWave(WaveData wave)
        {
            int chapter = LevelContext.CurrentChapter.chapter;
            bool firstSpawn = true;

            foreach (LevelEnemyConfiguration cfg in wave.enemies)
            {
                remainingEnemies += cfg.spawnCount;

                float factor = cfg.GetGrowthFactor(chapter);
                float hp = LevelManager.LevelScaler.GetHp(cfg.enemyData, factor);
                float damage = LevelManager.LevelScaler.GetDamage(cfg.enemyData, factor);

                for (int i = 0; i < cfg.spawnCount; i++)
                {
                    if (!firstSpawn && spawnInterval > 0f) yield return new WaitForSeconds(spawnInterval);
                    firstSpawn = false;

                    Vector2 pos = new(player.position.x + Random.Range(-maxOffset, maxOffset), player.position.y);
                    pos.x = LevelBounds.ClampX(pos.x);
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