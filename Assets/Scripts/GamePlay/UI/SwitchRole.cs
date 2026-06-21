using GamePlay.PlayerDataHandle;
using GamePlay.Role;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI
{
    public class SwitchRole : MonoBehaviour
    {
        [SerializeField] private Button switchButton;
        private PlayerInputHandler playerInputHandler;
        void Awake()
        {
            playerInputHandler = FindAnyObjectByType<PlayerInputHandler>();
            switchButton.onClick.AddListener(Switch);
        }

        private void Switch()
        {
            playerInputHandler.SwitchStrategy();
        }
        
        private void OnDisable()
        {
            switchButton.onClick.RemoveListener(Switch);
        }
    }
}