using GamePlay.EnemyConfiguration;
using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex;
        public LevelSprite sprite;
        public WaveData[] waves;
        
    }
}
