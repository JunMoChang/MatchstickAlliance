using UnityEngine;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public interface ISlotView
    {
        RectTransform RectTransform { get; }
        void Init();
    }
}