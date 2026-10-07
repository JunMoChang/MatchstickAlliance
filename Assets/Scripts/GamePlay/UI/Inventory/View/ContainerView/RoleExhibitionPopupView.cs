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
        [SerializeField] private GameObject content;
        [SerializeField] private Button closeButton;
        public event Action<RoleName> OnUnlockRole;
        public void Initialize()
        {
            closeButton.onClick.AddListener(Hide);
            int childCount = content.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
            
            PlayerDataManager playerDataManager = PlayerDataManager.Instance;
            foreach (RoleRegistry.RoleEntry? entry in GameDataManager.RoleRegistry.entries)
            {
                if(entry == null) continue;
                
                GameObject roleExhibition = Instantiate(roleExhibitionPrefab, content.transform);

                RoleExhibitionView view = roleExhibition.GetComponent<RoleExhibitionView>();
                view.OnUnlockRole += UnlockRole;
                view.SetData(entry.Value.template, playerDataManager.PlayerData.ownedRoles.ContainsKey(entry.Value.roleName));
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
        
        public void SetRoleUnlocked(RoleName roleName)
        {
            foreach (Transform child in content.transform)
            {
                RoleExhibitionView view = child.GetComponent<RoleExhibitionView>();
                if (view != null && view.RoleName == roleName)
                {
                    view.SetUnlocked();
                    return;
                }
            }
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