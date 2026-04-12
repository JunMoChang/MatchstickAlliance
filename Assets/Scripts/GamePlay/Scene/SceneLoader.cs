using System.Collections;
using GamePlay.GameModel.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.Scene
{
    public class SceneLoader :  MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        [SerializeField] private LevelContext levelContext;
        [SerializeField] string loadingSceneName = "LoadingScene";
        public static System.Action<float> OnLoadProgress;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void LoadLevel(ChapterData chapter, LevelData level)
        {
            levelContext.currentChapter = chapter;
            levelContext.currentLevel = level;
            
            StartCoroutine(LoadWithTransition(chapter.sceneName));
        }

        private IEnumerator LoadWithTransition(string sceneName)
        {
            SceneManager.LoadScene(loadingSceneName);
            yield return null; 
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            
            while (op.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(op.progress / 0.9f);
                yield return new WaitForSeconds(0.5f);
            }
            
            OnLoadProgress?.Invoke(1f);
            yield return new WaitForSeconds(0.5f); 
            
            op.allowSceneActivation = true;
        }
        
        public void LoadMainMenu()
        {
            StopAllCoroutines();
            SceneManager.LoadScene("MainMenu");
        }
    }
}