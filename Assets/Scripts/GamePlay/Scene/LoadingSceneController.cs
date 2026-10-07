using UnityEngine;

namespace GamePlay.Scene
{
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] RectTransform progressFill;
        private float lastT;
        
        private void OnEnable()
        {
            progressFill.anchorMin = Vector2.zero;
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
            
            SceneLoader.OnLoadProgress += UpdateProgress;
        }

        private void OnDisable()
        {
            SceneLoader.OnLoadProgress -= UpdateProgress;
        }
        
        private void UpdateProgress(float t)
        {
            if (progressFill == null) return;
            if(Mathf.Approximately(lastT, t)) return;
            
            lastT = t;
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
        }
    }
}
