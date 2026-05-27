using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Inventory.View
{
    public abstract class SlotsScrollView<TView, TData> : MonoBehaviour where TView : MonoBehaviour, ISlotView
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private RectTransform contentRect;

        [SerializeField] private int columnCount = 5;
        public Vector2 cellSize = new (150, 150);
        public Vector2 spacing = new (10, 10);
        public Vector2 padding = new (10, 10);
    
        private List<TView> slotPool;
        private IReadOnlyList<TData> displayDataList;
    
        private int poolRows;
        private float cellHeight;
        private float cellWidth;
    
        private int lastStartRow = -1;

        void Awake()
        {
            contentRect = scrollRect.content;
            scrollRect.onValueChanged.AddListener(OnScroll);
        
            Initialize();
        }   
    
        private void Initialize()
        {
            cellHeight = cellSize.y + spacing.y;
            cellWidth =  cellSize.x + spacing.x;
            
            int visibleRows = Mathf.CeilToInt(scrollRect.viewport.rect.height / cellHeight);
            
            poolRows = visibleRows + 2;
            slotPool =  new List<TView>(poolRows * columnCount);
            for (int i = 0; i < slotPool.Capacity; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, contentRect);
                RectTransform rt = slotObj.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = cellSize;
            
                TView slotView = slotObj.GetComponent<TView>();
                slotView.Init();
                slotPool.Add(slotView);
                PositionSlot(slotView, i);
            }
        }
    
        public void SetData(IReadOnlyList<TData> currentData, int dataContainerCapacity)
        {
            displayDataList = currentData;
            SetContentSize(dataContainerCapacity);
        
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0);
            lastStartRow = -1;
        
            RefreshVisibleSlots();
        }
    
        private void RefreshVisibleSlots()
        {
            if (!gameObject.activeInHierarchy || slotPool.Count == 0) return;
            
            float scrollY = contentRect.anchoredPosition.y; 
            int startRow = Mathf.FloorToInt(Mathf.Max(0, scrollY) / cellHeight);
        
            if (startRow == lastStartRow) return;
            
            lastStartRow = startRow;
            
            int endRow = Mathf.CeilToInt((scrollY + scrollRect.viewport.rect.height) / cellHeight);
            int startIndex = startRow * columnCount;
            for (int i = 0; i < slotPool.Count; i++)
            {
                int dataIndex = startIndex + i;
                TView slotView = slotPool[i];
            
                int dataRow = dataIndex / columnCount;
            
                PositionSlot(slotView, dataIndex);
                bool isInViewport = dataRow < endRow;
                if (isInViewport)
                {
                    slotView.gameObject.SetActive(true);
                
                    BindSlot(slotView, dataIndex < displayDataList.Count ? displayDataList[dataIndex] : default);
                }
                else
                {
                    slotView.gameObject.SetActive(false);
                }
            }
        }

        private void OnScroll(Vector2 scrollPos)
        {
            RefreshVisibleSlots();
        }
    
        private void SetContentSize(int count)
        {
            int rowCount = Mathf.CeilToInt((float)count / columnCount);
            float height = rowCount * cellHeight;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, height);
        }
        
        private void PositionSlot(TView view, int index)
        {
            int row = index / columnCount;
            int col = index % columnCount;
            view.RectTransform.anchoredPosition = new Vector2(cellWidth  * col + padding.x, -cellHeight * row);
            view.gameObject.SetActive(true);
        }
        
        protected abstract void BindSlot(TView view, TData data);
    }
}