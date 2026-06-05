using System;
using GamePlay.Role.RoleData;
using GamePlay.Role.RoleData.BaseData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.SingleView
{
    public class RoleExhibitionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roleNameText;
        [SerializeField] private Image roleImage;
        [SerializeField] private Button purchaseBtn;
        
        public RoleName RoleName { get; private set; }

        public event Action<RoleName> OnUnlockRole;

        public void SetData(RoleBaseData roleBaseData, bool isOwned)
        {
            RoleName = roleBaseData.roleName;
            roleNameText.text = roleBaseData.roleName.ToString();
            roleImage.sprite = roleBaseData.exhibitionIcon;
            purchaseBtn.GetComponentInChildren<TextMeshProUGUI>().text = roleBaseData.price.ToString();
            if (isOwned)
            {
                SetUnlocked();
            }
            else
            {
                LockRole();
            }
        }
        
        private void LockRole()
        {
            roleImage.color = new Color(0, 0, 0, 200 / 255f);

            purchaseBtn.onClick.AddListener(OnPurchaseClicked);
            purchaseBtn.interactable = true;
            purchaseBtn.gameObject.SetActive(true);
        }
        
        private void OnPurchaseClicked()
        {
            OnUnlockRole?.Invoke(RoleName);
        }
        
        public void SetUnlocked()
        {
            roleImage.color = new Color(1, 1, 1, 1);

            OnUnlockRole = null;
            purchaseBtn.onClick.RemoveListener(OnPurchaseClicked);
            purchaseBtn.interactable = false;
            purchaseBtn.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            purchaseBtn.onClick.RemoveListener(OnPurchaseClicked);
            OnUnlockRole = null;
        }
    }
}