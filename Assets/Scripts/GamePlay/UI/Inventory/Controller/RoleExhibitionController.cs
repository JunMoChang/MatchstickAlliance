using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.View.ContainerView;
using UnityEngine;

namespace GamePlay.UI.Inventory.Controller
{
    public class RoleExhibitionController : MonoBehaviour
    {
        [SerializeField] private RoleExhibitionPopupView roleExhibitionView;

        public void Initialize()
        {
            roleExhibitionView.Initialize();
            roleExhibitionView.OnUnlockRole += PurchaseRole;
        }
        
        public void Show()
        {
            roleExhibitionView.Show();
        }

        public void HidePopup()
        {
            roleExhibitionView.Hide();
        }
        private void PurchaseRole(RoleName roleName)
        {
            PlayerDataManager.Instance.UnLockNewRole(roleName);
        }
        
        
        void OnDestroy()
        {
            roleExhibitionView.OnUnlockRole -= PurchaseRole;
        }
    }
}