using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private EndedGame endedGame;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            SceneLoader.OnLevelLoaded += FindObjectType<EndedGame>;
        }

        public static void ShowLevelEndedPanel()
        {
            if (Instance.endedGame != null) Instance.endedGame.Show();
        }

        private void FindObjectType<T>()  where T : EndedGame
        {
            endedGame = FindAnyObjectByType<T>();
        }
    }
}
