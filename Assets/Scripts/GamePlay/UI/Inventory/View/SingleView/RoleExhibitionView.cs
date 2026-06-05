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
        
        public event Action<RoleName> OnUnlockRole;
        public void SetData(RoleBaseData roleBaseData, bool isOwned)
        {
            roleNameText.text = roleBaseData.roleName.ToString();
            roleImage.sprite = roleBaseData.exhibitionIcon;
            purchaseBtn.GetComponentInChildren<TextMeshProUGUI>().text = roleBaseData.price.ToString();
            if (isOwned)
            {
                UnLockRole();
            }
            else
            {
                LockRole();
            }
        }
        
        private void LockRole()
        {
            roleImage.color = new Color(1, 1, 1, 1);
            
            purchaseBtn.onClick.AddListener(UnLockRole);
            purchaseBtn.interactable = true;
            purchaseBtn.gameObject.SetActive(true);
        }
        
        private void UnLockRole()
        {
            roleImage.color = new Color(0,0,0, 235);
            
            Enum.TryParse(roleNameText.text, out RoleName roleName) ;
            OnUnlockRole?.Invoke(roleName);
            OnUnlockRole = null;
            
            purchaseBtn.onClick.RemoveAllListeners();
            purchaseBtn.interactable = false;
            purchaseBtn.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            purchaseBtn.onClick.RemoveAllListeners();
            OnUnlockRole = null;
        }
    }
}