using GamePlay.UI.Inventory.View;
using GamePlay.UI.Inventory.View.SingleView;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Inventory.Controller
{
    public class ItemInteractionController : MonoBehaviour, IPointerClickHandler
    {
        private ItemSlotView slotView;
        
        void Start()
        {
            slotView = GetComponent<ItemSlotView>();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (slotView.currentDataModel == null || slotView.currentDataModel.IsEmpty()) return;
        
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                slotView.NotifyClicked();
            }
        }
    }
} 