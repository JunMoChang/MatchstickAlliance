using GamePlay.EnemyConfiguration;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData",  order = 4)]
    public class WaveData : ScriptableObject
    {
        public LevelEnemyConfiguration[] enemies;
        public float triggerX; 
    }
}