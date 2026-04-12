using GamePlay.EnemyConfiguration;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex;
        public LevelSprite sprite;
        public LevelEnemyConfiguration[] enemy;
    }
}
