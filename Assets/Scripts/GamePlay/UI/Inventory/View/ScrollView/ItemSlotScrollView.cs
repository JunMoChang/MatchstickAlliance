using System;
using GamePlay.UI.Inventory.Model;
using GamePlay.UI.Inventory.View.SingleView;

namespace GamePlay.UI.Inventory.View.ScrollView
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