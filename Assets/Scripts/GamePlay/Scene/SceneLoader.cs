using System;
using System.Collections;
using GamePlay.GameModel.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.Scene
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] string loadingSceneName = "LoadingScene";
        
        /// <summary>
        /// 当前事件剩余待完成数
        /// </summary>
        private int pendingReadyCount;
        /// <summary>
        /// 关卡加载事件的订阅者总数
        /// </summary>
        private static int totalLevelSubscribers;
        /// <summary>
        /// 主菜单加载事件的订阅者总数
        /// </summary>
        private static int totalMainMenuSubscribers;
        
        /// <summary>
        /// 场景加载过渡事件
        /// </summary>
        public static event Action<float> OnLoadProgress;

        /// <summary>
        /// 关卡加载事件
        /// </summary>
        private Action prepareLevel;
        public static event Action OnPrepareLevel
        {
            add
            {
                Instance.prepareLevel += value;
                totalLevelSubscribers++;
            }
            remove
            {
                Instance.prepareLevel -= value;
                totalLevelSubscribers--;
            }
        }
        
        /// <summary>
        /// 主菜单加载事件
        /// </summary>
        private Action prepareMainMenu;
        public static event Action OnPrepareMainMenu
        {
            add
            {
                Instance.prepareMainMenu += value;
                totalMainMenuSubscribers++;
            }
            remove
            {
                Instance.prepareMainMenu -= value;
                totalMainMenuSubscribers--;
            }
        }
        
        /// <summary>
        /// 退出关卡事件
        /// </summary>
        public static event Action OnLevelExit;
        
        /// <summary>
        /// 场景加载完毕事件
        /// </summary>
        public static event Action OnLevelLoaded;
        
        private bool isReturningToMenu; 
        
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
            OnLevelExit?.Invoke();
            isReturningToMenu = false;
            LevelContext.CurrentChapter = chapter;
            LevelContext.CurrentLevel = level;
            StopAllCoroutines();
            StartCoroutine(LoadWithTransition(chapter.sceneName));
        }
        public void LoadMainMenu()
        {
            isReturningToMenu = true;
            OnLevelExit?.Invoke();
            StopAllCoroutines();
            StartCoroutine(LoadWithTransition("Main"));
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
            
            if (isReturningToMenu)
            {
                pendingReadyCount = totalMainMenuSubscribers;
                TriggerPrepare(prepareMainMenu);
            }
            else
            {
                pendingReadyCount = totalLevelSubscribers;
                TriggerPrepare(prepareLevel);
            }
            
            yield return new WaitUntil(() => pendingReadyCount <= 0);

            op.allowSceneActivation = true;
            yield return null;
            if(!isReturningToMenu)
                OnLevelLoaded?.Invoke();
        }

        private void TriggerPrepare(Action actionDel)
        {
            if (actionDel == null) return;
            Delegate[] delegates = actionDel.GetInvocationList();
            if(delegates.Length <= 0) return;
            foreach (Delegate del in delegates)
            {
                Action func = (Action)del;
                try
                {
                    func();
                }
                catch (Exception e)
                {
                    Debug.LogError($"SceneLoader: subscriber threw in prepare phase: {e}");
                }
                finally
                {
                    pendingReadyCount--;
                }
            }
        }
        
    }
}