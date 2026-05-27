using System;
using System.Collections.Generic;
using GamePlay.Scene;
using UnityEngine;

namespace GamePlay.UI.BackgroundParallax
{
    [RequireComponent(typeof(Canvas))]
    public class BattleBackgroundController : MonoBehaviour
    {
        [Serializable]
        public class MountainLayerConfig
        {
            [Range(0f, 1f)] public float parallaxFactor = 0.5f;
        }

        [Header("Layer References")]
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform mountainContainer;
        [SerializeField] private RectTransform groundRect;

        [Header("Parallax Factors")]
        [SerializeField] private MountainLayerConfig farMountain = new() { parallaxFactor = 0.15f };
        [SerializeField] private MountainLayerConfig middleMountain = new() { parallaxFactor = 0.40f };
        [SerializeField] private MountainLayerConfig nearMountain = new() { parallaxFactor = 0.70f };

        [Header("Scroll Settings")]
        [SerializeField] private float scrollSensitivity = 1f;

        private Transform playerTransform;
        private bool playerFound;
        private float canvasWidth;
        private float worldToCanvasUnit;
        private float spawnX;
        private float scrollableRange;
        private float maxScrollOffset;
        private float minScrollOffset;

        private Vector2 backgroundInitialPos;
        private Vector2 groundInitialPos;

        private readonly List<MountainEntry> farMountains = new ();
        private readonly List<MountainEntry> middleMountains = new ();
        private readonly List<MountainEntry> nearMountains  = new ();
        
        private struct MountainEntry
        {
            public RectTransform rect;
            public Vector2 initialAnchoredPosition;
        }

        private void Start()
        {
            RectTransform canvasRect = (RectTransform)transform;
            canvasWidth = canvasRect.rect.width;
            float refHeight = canvasRect.rect.height;

            Camera cam = Camera.main;
            float orthoSize = cam != null? cam.orthographicSize : 5f;
            worldToCanvasUnit = refHeight / (2f * orthoSize);

            SpawnPoint sp = FindAnyObjectByType<SpawnPoint>();
            spawnX = sp != null ? sp.transform.position.x : 0f;

            CacheInitialPositions();
            CalculateBackgroundExtent();
            GroupMountains();
            FindPlayer();
        }
        
        private void Update()
        {
            if (!playerFound)
            {
                FindPlayer();
                if (!playerFound) return;
            }

            if (scrollableRange <= 0f) return;

            float playerDeltaX = playerTransform.position.x - spawnX;
            float targetOffset = -playerDeltaX * worldToCanvasUnit * scrollSensitivity;
            float baseScroll = Mathf.Clamp(targetOffset, minScrollOffset, maxScrollOffset);
            
            {
                Vector2 pos = backgroundInitialPos;
                pos.x += baseScroll;
                backgroundRect.anchoredPosition = pos;
            }
            
            {
                Vector2 pos = groundInitialPos;
                pos.x += baseScroll;
                groundRect.anchoredPosition = pos;
            }

            ApplyMountainOffset(farMountains, baseScroll * farMountain.parallaxFactor);
            ApplyMountainOffset(middleMountains, baseScroll * middleMountain.parallaxFactor);
            ApplyMountainOffset(nearMountains, baseScroll * nearMountain.parallaxFactor);
        }
        
        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerFound = true;
            }
        }

        private void CacheInitialPositions()
        {
            if (backgroundRect != null) backgroundInitialPos = backgroundRect.anchoredPosition;
            if (groundRect != null) groundInitialPos = groundRect.anchoredPosition;
        }

        private void CalculateBackgroundExtent()
        {
            if (backgroundRect == null || backgroundRect.childCount == 0) return;

            float minX = float.MaxValue;
            float maxX = float.MinValue;

            for (int i = 0; i < backgroundRect.childCount; i++)
            {
                RectTransform child = backgroundRect.GetChild(i) as RectTransform;
                if (child == null) continue;

                float halfW = child.rect.width * 0.5f;
                float childLeft  = child.anchoredPosition.x - halfW;
                float childRight = child.anchoredPosition.x + halfW;

                minX = Mathf.Min(minX, childLeft);
                maxX = Mathf.Max(maxX, childRight);
            }

            float totalWidth = maxX - minX;
            scrollableRange = Mathf.Max(0, totalWidth - canvasWidth);
            
            maxScrollOffset = -canvasWidth * 0.5f - minX;
            minScrollOffset =  canvasWidth * 0.5f - maxX;
        }

        private void GroupMountains()
        {
            if (mountainContainer == null) return;

            for (int i = 0; i < mountainContainer.childCount; i++)
            {
                RectTransform child = mountainContainer.GetChild(i) as RectTransform;
                if (child == null) continue;

                var entry = new MountainEntry
                {
                    rect = child,
                    initialAnchoredPosition = child.anchoredPosition
                };

                string nm = child.name;
                if (nm.Contains("Far"))
                    farMountains.Add(entry);
                else if (nm.Contains("Middle"))
                    middleMountains.Add(entry);
                else if (nm.Contains("Near"))
                    nearMountains.Add(entry);
            }
        }

        private static void ApplyMountainOffset(List<MountainEntry> entries, float offset)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Vector2 pos = entries[i].initialAnchoredPosition;
                pos.x += offset;
                entries[i].rect.anchoredPosition = pos;
            }
        }
    }
}
