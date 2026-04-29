using UnityEngine;

namespace GamePlay.UI.BackgroundParallax
{
    [RequireComponent(typeof(Canvas))]
    public class BattleBackgroundController : MonoBehaviour
    {
        [Header("视差层")]
        [SerializeField] RectTransform mountainFar;
        [SerializeField] RectTransform mountainNear;

        [Header("云")]
        [SerializeField] RectTransform[] clouds;
        [SerializeField] float[] cloudSpeeds;

        private const float FarActor  = 0.15f;
        private const float NearActor = 0.35f;

        private float canvasWidth;
        private float[] cloudX;

        private void Start()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvasWidth = (canvas.transform as RectTransform).rect.width;
            cloudX = new float[clouds.Length];
            for (int i = 0; i < clouds.Length; i++)
                cloudX[i] = clouds[i].anchoredPosition.x;
        }
        
        public void SetParallaxOffset(float offset)
        {
            mountainFar.anchoredPosition  = new Vector2(offset * FarActor,  mountainFar.anchoredPosition.y);
            mountainNear.anchoredPosition = new Vector2(offset * NearActor, mountainNear.anchoredPosition.y);
        }

        private void Update()
        {
            MoveClouds();
        }

        private void MoveClouds()
        {
            for (int i = 0; i < clouds.Length; i++)
            {
                float speed = i < cloudSpeeds.Length ? cloudSpeeds[i] : 60f;
                cloudX[i] -= speed * Time.deltaTime;

                float halfCanvas = canvasWidth * 0.5f;
                float cloudWidth = clouds[i].rect.width;
                if (cloudX[i] + cloudWidth * 0.5f < -halfCanvas)
                    cloudX[i] = halfCanvas + cloudWidth * 0.5f;

                clouds[i].anchoredPosition = new Vector2(cloudX[i], clouds[i].anchoredPosition.y);
            }
        }
    }
}