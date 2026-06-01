using UnityEngine;

namespace GamePlay.UI.Inventory.View
{
    public interface ISlotView
    {
        RectTransform RectTransform { get; }
        void Init();
    }
}