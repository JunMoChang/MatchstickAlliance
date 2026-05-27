using UnityEngine.Serialization;

namespace GamePlay.UI
{
    [System.Serializable]
    public class FunctionButtonData
    {
        public FunctionButtonName nameLabel;
        public UnityEngine.Sprite icon;
        public int badge;
        public bool badgeExclamation;
    }

    public enum FunctionButtonName
    {
        装备,
        背包,
        技能,
        强化,
        角色
    }
}