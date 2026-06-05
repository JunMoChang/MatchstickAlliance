using System;
using AssetLoad;
using GamePlay.PlayerDataHandle;
using GamePlay.Role.RoleData;
using GamePlay.UI.Inventory.View.SingleView;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.Inventory.View.ContainerView
{
    public class RoleExhibitionPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject roleExhibitionPrefab;
        [SerializeField] private GameObject roleContent;
        [SerializeField] private Button closeButton;
        public event Action<RoleName> OnUnlockRole;
        public void Initialize()
        {
            closeButton.onClick.AddListener(Hide);
            int childCount = roleContent.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(roleContent.transform.GetChild(i).gameObject);
            }
            
            PlayerDataManager playerDataManager = PlayerDataManager.Instance;
            foreach (RoleRegistry.RoleEntry entry in GameDataManager.RoleRegistry.entries)
            {
                GameObject roleExhibition = Instantiate(roleExhibitionPrefab, roleContent.transform);

                RoleExhibitionView view = roleExhibition.GetComponent<RoleExhibitionView>();
                view.OnUnlockRole += UnlockRole;
                view.SetData(entry.template, playerDataManager.PlayerData.ownedRoles.ContainsKey(entry.roleName));
            }
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void UnlockRole(RoleName roleName)
        {
            OnUnlockRole?.Invoke(roleName);
        }

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(Hide);
        }
    }
}