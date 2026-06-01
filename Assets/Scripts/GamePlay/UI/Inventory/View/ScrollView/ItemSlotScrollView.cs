using System;
using GamePlay.Inventory.Model;

namespace GamePlay.UI.Inventory.View
{
    public class ItemSlotScrollView : SlotScrollView<ItemSlotView, ItemDataModel>
    {
        public event Action<ItemSlotView> OnSlotClicked;
        
        protected override void BindSlot(ItemSlotView view, ItemDataModel data)
        {
            view.OnClicked -= OnSlotClickedHandler;
            view.OnClicked += OnSlotClickedHandler;
            
            view.SetData(data);
        }
        
        private void OnSlotClickedHandler(ItemSlotView slot)
        {
            OnSlotClicked?.Invoke(slot);  
        } 
    }
}