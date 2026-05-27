using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "LevelSprite", menuName = "Scriptable Objects/LevelSprite",  order = 3)]
    public class LevelSprite : ScriptableObject
    {
        public Sprite levelSprite;
        public Sprite unlockedSprite;
    }
}
