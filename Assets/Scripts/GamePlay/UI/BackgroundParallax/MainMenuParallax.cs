using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.UI.BackgroundParallax
{
    public class MainMenuParallax : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public RectTransform rect;
            [Range(0f, 1f)] public float parallaxFactor = 0.5f;
        }

        [Header("Layer References")]
        [SerializeField] private ParallaxLayer[] layers;

        [Header("Parallax Limits (pixels, match image bleed)")]
        [SerializeField] private float maxOffsetRight = 10f;
        [SerializeField] private float maxOffsetLeft  = 760f;
        [SerializeField] private float maxOffsetUp    = 120f;
        [SerializeField] private float maxOffsetDown  = 0f;

        private Vector2[] initialPositions;
        private Vector2   pressOrigin;
        private Vector2   accumulatedOffset;

        private void Start()
        {
            initialPositions = new Vector2[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i].rect != null)
                    initialPositions[i] = layers[i].rect.anchoredPosition;
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;

            if (mouse.rightButton.wasPressedThisFrame) pressOrigin = mouse.position.ReadValue();

            if (mouse.rightButton.wasReleasedThisFrame)
            {
                Vector2 cur = mouse.position.ReadValue();
                accumulatedOffset += cur - pressOrigin;
                accumulatedOffset = ClampOffset(accumulatedOffset);
            }

            if (!mouse.rightButton.isPressed) return;

            Vector2 offset = ClampOffset(accumulatedOffset + (mouse.position.ReadValue() - pressOrigin));

            for (int i = 0; i < layers.Length; i++)
            {
                if (!layers[i].rect) continue;
                Vector2 pos = initialPositions[i];
                pos.x += offset.x * layers[i].parallaxFactor;
                pos.y += offset.y * layers[i].parallaxFactor;
                layers[i].rect.anchoredPosition = pos;
            }
        }

        private Vector2 ClampOffset(Vector2 offset)
        {
            offset.x = Mathf.Clamp(offset.x, -maxOffsetLeft, maxOffsetRight);
            offset.y = Mathf.Clamp(offset.y, -maxOffsetDown, maxOffsetUp);
            return offset;
        }
    }
}