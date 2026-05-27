using System;
using GamePlay.Inventory.Model;

namespace GamePlay.Inventory.View
{
    public class ItemSlotsScrollView : SlotsScrollView<ItemSlotView, ItemDataModel>
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