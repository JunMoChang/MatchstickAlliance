using UnityEngine;

namespace GamePlay.Scene
{
    public class LevelBounds : MonoBehaviour
    {
        [SerializeField] private Collider2D leftWall;
        [SerializeField] private Collider2D rightWall;

        private static LevelBounds current;
        private float minX;
        private float maxX;
        private float lastAspect;

        /// <summary>
        /// 钳制水平坐标在边界内
        /// </summary>
        public static float ClampX(float x)
        {
            if (current == null) return x;
            return Mathf.Clamp(x, current.minX, current.maxX);
        }

        private void OnEnable()
        {
            current = this;
            lastAspect = -1f;
            Refresh();
        }

        private void OnDisable()
        {
            if (current == this) current = null;
        }
        
        private void Update()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            if (Mathf.Approximately(cam.aspect, lastAspect)) return;
            lastAspect = cam.aspect;
            Refresh();
        }

        private void Refresh()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float halfWidth = cam.orthographicSize * cam.aspect;
            minX = cam.transform.position.x - halfWidth;
            maxX = cam.transform.position.x + halfWidth;
            
            if (leftWall != null) minX = Mathf.Max(minX, leftWall.bounds.center.x);
            if (rightWall != null) maxX = Mathf.Min(maxX, rightWall.bounds.center.x);
        }
    }
}
