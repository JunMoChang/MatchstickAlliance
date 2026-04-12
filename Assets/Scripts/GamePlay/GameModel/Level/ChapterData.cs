using UnityEngine;

namespace GamePlay.GameModel.Level
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "Scriptable Objects/ChapterData")]
    public class ChapterData :  ScriptableObject
    {
        public int chapter;
        public string sceneName;
        public LevelData[] levelData;
    }
}