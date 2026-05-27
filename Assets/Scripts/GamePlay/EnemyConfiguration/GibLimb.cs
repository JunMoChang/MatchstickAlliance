using System.Collections;
using UnityEngine;

namespace GamePlay.EnemyConfiguration
{
    public class GibLimb : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sr;
        public void Init(float lifetime, float fadeStartRatio)
        {
            StartCoroutine(FadeDestroy(lifetime, fadeStartRatio));
        }

        private IEnumerator FadeDestroy(float lifetime, float fadeRatio)
        {
            yield return new WaitForSeconds(lifetime * fadeRatio);
            
            float fadeDuration = lifetime * (1f - fadeRatio);
            float elapsed = 0f;
            Color c = sr.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
