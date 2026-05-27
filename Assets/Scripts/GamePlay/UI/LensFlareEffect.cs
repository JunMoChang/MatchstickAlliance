using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class LensFlareEffect : MonoBehaviour
    {
        [Tooltip("呼吸周期")]
        public float duration = 2.0f;
        public float minScale = 0.9f;
        public float maxScale = 1.1f;
        public float minAlpha = 0.7f;
        public float maxAlpha = 1.0f;

        private Image image;

        void Start()
        {
            image = GetComponent<Image>();
        }

        void Update()
        {
            float t = Mathf.Sin( 2 * Mathf.PI * Time.time / duration);
            float normalized = (t + 1) / 2; 
            
            float currentScale = Mathf.Lerp(minScale, maxScale, normalized);
            transform.localScale = Vector3.one * currentScale;
            
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, normalized);
            Color c = image.color;
            c.a = currentAlpha;
            image.color = c;
        }
    }
}