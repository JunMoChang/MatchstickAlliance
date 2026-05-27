using UnityEngine;

namespace GamePlay.Inventory.View
{
    public interface ISlotView
    {
        RectTransform RectTransform { get; }
        GameObject GameObject { get; }
        void Init();
    }
}