using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Inventory.Controller
{
    public class ItemInteractionController : MonoBehaviour, IPointerClickHandler
    {
        private View.ItemSlotView slotView;
        
        void Start()
        {
            slotView = GetComponent<View.ItemSlotView>();
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