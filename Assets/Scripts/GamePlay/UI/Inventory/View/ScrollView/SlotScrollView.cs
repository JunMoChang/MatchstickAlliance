using System.Collections.Generic;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.SingleView;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.ScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class SlotScrollView<TView, TData> : MonoBehaviour where TView : MonoBehaviour, ISlotView where TData : ItemDataModel
    {
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private int columnCount = 5;
        
        private ScrollRect scrollRect;
        public Vector2 cellSize = new (150, 150);
        public Vector2 spacing = new (10, 10);
        public Vector2 padding = new (10, 10);
    
        private List<TView> slotPool;
        private IReadOnlyList<TData> displayDataList;
    
        private int poolRows;
        private float cellHeight;
        private float cellWidth;
    
        private int lastStartRow = -1;
        private bool isInitialized;

        public void Initialize()
        {
            scrollRect = GetComponent<ScrollRect>();
            contentRect = scrollRect.content;
            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void EnsureInitialized()
        {
            if (isInitialized || scrollRect == null) return;
            isInitialized = true;

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
            EnsureInitialized();

            displayDataList = currentData;
            SetContentSize(dataContainerCapacity);

            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0);
            lastStartRow = -1;

            RefreshVisibleSlots();
        }

        private void RefreshVisibleSlots()
        {
            if (!gameObject.activeInHierarchy || slotPool == null || slotPool.Count == 0) return;

            float scrollY = contentRect.anchoredPosition.y;
            int startRow = Mathf.FloorToInt(Mathf.Max(0, scrollY) / cellHeight);

            int deltaRows = startRow - lastStartRow;
            if (deltaRows == 0) return;

            int endRow = Mathf.CeilToInt((scrollY + scrollRect.viewport.rect.height) / cellHeight);
            
            if (lastStartRow < 0 || Mathf.Abs(deltaRows) >= poolRows)
                FullRefresh(startRow, endRow);
            else if (deltaRows > 0)
                ScrollDown(deltaRows, startRow, endRow);
            else
                ScrollUp(-deltaRows, startRow, endRow);
            

            lastStartRow = startRow;
        }

        private void FullRefresh(int startRow, int endRow)
        {
            int startIndex = startRow * columnCount;
            for (int i = 0; i < slotPool.Count; i++)
            {
                int dataIndex = startIndex + i;
                TView slotView = slotPool[i];
                int dataRow = dataIndex / columnCount;

                PositionSlot(slotView, dataIndex);
                BindSlot(slotView, dataIndex < displayDataList.Count ? displayDataList[dataIndex] : null);
                slotView.gameObject.SetActive(dataRow < endRow);
            }
        }

        private void ScrollDown(int deltaRows, int startRow, int endRow)
        {
            int moveCount = deltaRows * columnCount;
            int poolSize = slotPool.Count;
            int startIndex = startRow * columnCount;
            
            for (int i = 0; i < moveCount; i++)
            {
                TView slot = slotPool[0];
                slotPool.RemoveAt(0);
                slotPool.Add(slot);
            }
            
            for (int i = 0; i < moveCount; i++)
            {
                int poolIndex = poolSize - moveCount + i;
                int dataIndex = startIndex + poolIndex;
                TView slotView = slotPool[poolIndex];

                PositionSlot(slotView, dataIndex);
                BindSlot(slotView, dataIndex < displayDataList.Count ? displayDataList[dataIndex] : null);
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                int dataRow = (startIndex + i) / columnCount;
                slotPool[i].gameObject.SetActive(dataRow < endRow);
            }
        }

        private void ScrollUp(int deltaRows, int startRow, int endRow)
        {
            int moveCount = deltaRows * columnCount;
            int poolSize = slotPool.Count;
            int startIndex = startRow * columnCount;
            
            for (int i = 0; i < moveCount; i++)
            {
                TView slot = slotPool[poolSize - 1];
                slotPool.RemoveAt(poolSize - 1);
                slotPool.Insert(0, slot);
            }
            
            for (int i = 0; i < moveCount; i++)
            {
                int dataIndex = startIndex + i;
                TView slotView = slotPool[i];

                PositionSlot(slotView, dataIndex);
                BindSlot(slotView, dataIndex < displayDataList.Count ? displayDataList[dataIndex] : null);
            }

            for (int i = 0; i < poolSize; i++)
            {
                int dataRow = (startIndex + i) / columnCount;
                slotPool[i].gameObject.SetActive(dataRow < endRow);
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