using System;
using GamePlay.Inventory.Model;
using GamePlay.UI;

namespace GamePlay.Inventory.View
{
    public class EquipmentInfoSlotsScrollView : SlotsScrollView<EquipmentInfoSlotView, ItemDataModel>
    {
        public event Action<ItemDataModel> OnSlotFunctionButtonClicked;
        private FunctionButtonName curFunBtnNm;

        public void SetFunctionButtonName(FunctionButtonName funBtnNm)
        {
            curFunBtnNm = funBtnNm;
        }

        protected override void BindSlot(EquipmentInfoSlotView infoView, ItemDataModel data)
        {
            infoView.OnFunctionButtonClicked -= OnSlotButtonClicked;
            infoView.OnFunctionButtonClicked += OnSlotButtonClicked;
            infoView.SetData(data, curFunBtnNm);
        }
        
        private void OnSlotButtonClicked(ItemDataModel data)
        {
            OnSlotFunctionButtonClicked?.Invoke(data);
        }
    }
}