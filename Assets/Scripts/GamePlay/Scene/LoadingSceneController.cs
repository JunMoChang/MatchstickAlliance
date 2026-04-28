using UnityEngine;
using UnityEngine.UIElements;

namespace GamePlay.Scene
{
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        private ProgressBar progressBar;
        
        private void OnEnable()
        {
            progressBar = uiDocument.rootVisualElement.Q<ProgressBar>("progress-bar");
            SceneLoader.OnLoadProgress += UpdateProgress;
        }
        private void OnDisable() => SceneLoader.OnLoadProgress -= UpdateProgress;

        private void UpdateProgress(float t)
        {
            progressBar.value = t * 100f;
        } 
    }
}