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
        /// <summary> 进度条从 0 到 1 的最短时间（秒）/// </summary>
        [SerializeField, Min(0f)] float minLoadDuration = 1f;
        /// <summary> 进度条到 100% 后，切换场景前的停顿（秒）/// </summary>
        [SerializeField] float holdAtFullDuration = 0.3f;
        private WaitForSecondsRealtime secondsRealtime;
        /// <summary>
        /// 等待订阅者上报就绪的最长时间（秒）。超时后记录错误并放行场景激活，
        /// 避免任何漏报（订阅者异常、被销毁、分支提前 return）导致永久卡在加载场景
        /// </summary>
        [SerializeField, Min(1f)] float readyTimeout = 10f;
        
        /// <summary>
        /// 当前事件剩余待上报数。订阅者必须自行调用 ReportReady 递减，SceneLoader 不再代为递减——
        /// 否则异步订阅者（协程 / await 加载资源）会被提前算作就绪
        /// </summary>
        private int pendingReadyCount;
        /// <summary> 加载流程版本号：每次场景切换递增。订阅者的异步准备可能在新的加载流程开始之后才完成 </summary>
        private int prepareVersion;

        /// <summary> 当前加载流程版本，订阅者在异步准备结束后据此校验 </summary>
        public static int PrepareVersion => Instance != null ? Instance.prepareVersion : 0;
        
        /// <summary> 场景加载过渡事件 </summary>
        public static event Action<float> OnLoadProgress;
        /// <summary> 关卡加载事件 </summary>
        private Action prepareLevel;
        public static event Action OnPrepareLevel
        {
            add => Instance.prepareLevel += value;
            remove => Instance.prepareLevel -= value;
        }
        /// <summary> 主菜单加载事件 </summary>
        private Action prepareMainMenu;
        public static event Action OnPrepareMainMenu
        {
            add => Instance.prepareMainMenu += value;
            remove => Instance.prepareMainMenu -= value;
        }
        /// <summary> 退出关卡事件 </summary>
        public static event Action OnLevelExit;
        /// <summary> 场景加载完毕事件 </summary>
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

        public void LoadScene(string sceneName)
        {
            StopAllCoroutines();
            StartCoroutine(LoadWithTransition(sceneName));
        }
        private IEnumerator LoadWithTransition(string sceneName)
        {
            // 新的加载流程开始：上一代在途的异步准备作废
            prepareVersion++;
            
            SceneManager.LoadScene(loadingSceneName);
            yield return null;
            
            OnLoadProgress?.Invoke(0f);
            TriggerPrepare(isReturningToMenu ? prepareMainMenu : prepareLevel);
            yield return WaitForReady(sceneName);
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                op.allowSceneActivation = false;

                float progress = 0f;
                while (op.progress < 0.9f || progress < 1f)
                {
                    float target = Mathf.Clamp01(op.progress / 0.9f);
                    progress = Mathf.MoveTowards(progress, target, Time.unscaledDeltaTime / minLoadDuration);
                    OnLoadProgress?.Invoke(progress);
                    yield return null;
                }

                OnLoadProgress?.Invoke(1f);
                secondsRealtime ??= new WaitForSecondsRealtime(holdAtFullDuration);
                yield return secondsRealtime;

                op.allowSceneActivation = true;
            }

            yield return null;
            
            if(!isReturningToMenu) OnLevelLoaded?.Invoke();
        }
        
        /// <summary>
        /// 逐个调用订阅者，并以实际订阅者数量作为待上报数（不依赖静态计数，避免与实例上的委托列表不同步）。
        /// 订阅者无论同步还是异步，都必须在准备完成后自行调用 ReportReady 上报；
        /// 这里不再代为递减，否则协程/await 形式的异步订阅者会在资源还没加载完时就被算作就绪。
        /// </summary>
        private void TriggerPrepare(Action actionDel)
        {
            Delegate[] delegates = actionDel?.GetInvocationList();
            pendingReadyCount = delegates?.Length ?? 0;

            Debug.Log($"[SceneLoader] 准备阶段开始：订阅者数 {pendingReadyCount} 个");

            if (pendingReadyCount <= 0) return;

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
                    
                    ReportReadyInternal();
                }
            }
        }

        /// <summary>
        /// 订阅者完成准备后上报。异步准备（协程、await 加载资源）必须在加载完成后调用，
        /// </summary>
        public static void ReportReady()
        {
            if (Instance == null) return;
            
            Instance.ReportReadyInternal();
        }

        private void ReportReadyInternal()
        {
            if (pendingReadyCount <= 0)
            {
                Debug.LogWarning("[SceneLoader] ReportReady 次数多于订阅者数量，已忽略");
                return;
            }
            pendingReadyCount--;
            Debug.Log($"[SceneLoader] 收到就绪上报，剩余 {pendingReadyCount} 路");
        }

        /// <summary>
        /// 等待订阅者上报就绪。每帧轮询、不阻塞主线程（异步订阅者需要主线程继续跑才能完成加载），
        /// 超时只记录错误并放行，不做阻塞
        /// </summary>
        private IEnumerator WaitForReady(string sceneName)
        {
            if (pendingReadyCount <= 0) yield break;

            Debug.Log($"[SceneLoader] 等待 {pendingReadyCount} 路订阅者上报就绪");

            float elapsed = 0f;
            while (pendingReadyCount > 0)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= readyTimeout)
                {
                    Debug.LogError($"[SceneLoader] 等待订阅者准备就绪超时（{readyTimeout:F1}s），仍有 {pendingReadyCount} 路未上报，强制继续加载场景 {sceneName}");
                    yield break;
                }
                yield return null;
            }
        }
    }
}