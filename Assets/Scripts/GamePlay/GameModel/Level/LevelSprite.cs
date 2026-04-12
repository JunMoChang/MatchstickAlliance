using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelSprite", menuName = "Scriptable Objects/LevelSprite")]
    public class LevelSprite : ScriptableObject
    {
        public Sprite levelSprite;
    }
}
