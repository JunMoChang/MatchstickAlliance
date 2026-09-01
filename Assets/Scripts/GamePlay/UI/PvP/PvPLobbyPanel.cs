using System.Collections;
using System.Collections.Generic;
using AssetLoad;
using Fusion;
using GamePlay.GameModel.Level;
using GamePlay.PlayerDataHandle;
using GamePlay.PvP;
using GamePlay.Role.RoleData;
using GamePlay.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UI.PvP
{
    public class PvPLobbyPanel : MonoBehaviour
    {
        [Header("大厅")]
        [SerializeField] private GameObject panelRoot;

        [Header("创建房间")] 
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private GameObject roomPanel;
        [SerializeField] private TextMeshProUGUI roomCodeText;

        [Header("加入房间")]
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private GameObject joinRoomPanel;
        [SerializeField] private TMP_InputField roomCodeInput;
        [SerializeField] private Button confirmJoinButton;

        [Header("等待中")]
        [SerializeField] private GameObject loadingCoin;
        [SerializeField] private TextMeshProUGUI waitingStatusText;

        [Header("角色选择")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Button roleASelectBtn;
        [SerializeField] private Button roleBSelectBtn;
        [SerializeField] private Image roleAImage;
        [SerializeField] private Image roleBImage;
        [SerializeField] private TextMeshProUGUI playerARoleNameText;
        [SerializeField] private TextMeshProUGUI playerBRoleNameText;
        [SerializeField] private RoleSelectPopupView roleSelectPopupView;

        [Header("就绪 & 开始")]
        [SerializeField] private Button startBattleButton;
        
        private List<RoleRegistry.RoleEntry> ownedRoleEntries;
        private RoleRegistry.RoleEntry? localSelectedRole;
        private string opponentRoleName;
        private bool pvpEventsRegistered;

        public void Show()
        {
            panelRoot.SetActive(true);
            RegisterPvPEvents();
            ResetUI();
            LoadOwnedRoles();
            
            // 在场景加载前完成订阅，所有端就绪后启用输入
            FindAnyObjectByType<PlayerController>()?.PrepareForPvP();
        }

        public void Hide()
        {
            panelRoot.SetActive(false);
        }

        private void ResetUI()
        {
            lobbyPanel.SetActive(true);
            roomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            loadingCoin.SetActive(false);
            waitingStatusText.gameObject.SetActive(false);
            startBattleButton.interactable = false;
            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;

            // 清空角色选择状态
            localSelectedRole = null;
            opponentRoleName = null;
            if (playerARoleNameText != null) playerARoleNameText.text = "";
            if (playerBRoleNameText != null) playerBRoleNameText.text = "";
            if (roleAImage != null) roleAImage.sprite = null;
            if (roleBImage != null) roleBImage.sprite = null;
        }

        private void OnEnable()
        {
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
            confirmJoinButton.onClick.AddListener(OnConfirmJoinClicked);
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
            roleASelectBtn.onClick.AddListener(OnRoleASelectClicked);
            roleBSelectBtn.onClick.AddListener(OnRoleBSelectClicked);
            
            RegisterPvPEvents();
        }

        private void OnDisable()
        {
            createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
            joinRoomButton.onClick.RemoveListener(OnJoinRoomClicked);
            confirmJoinButton.onClick.RemoveListener(OnConfirmJoinClicked);
            startBattleButton.onClick.RemoveListener(OnStartBattleClicked);
            roleASelectBtn.onClick.RemoveListener(OnRoleASelectClicked);
            roleBSelectBtn.onClick.RemoveListener(OnRoleBSelectClicked);

            UnregisterPvPEvents();
        }

        private void RegisterPvPEvents()
        {
            if (pvpEventsRegistered) return;
            if (PvPNetworkManager.Instance == null) return;

            PvPNetworkManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
            PvPNetworkManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
            PvPNetworkManager.Instance.OnConnectedToServerEvent += OnNetworkConnected;
            PvPNetworkManager.Instance.OnShutdownEvent += OnNetworkShutdown;
            PvPNetworkManager.Instance.OnLobbyRoleReceived += OnOpponentRoleReceived;
            pvpEventsRegistered = true;
        }

        private void UnregisterPvPEvents()
        {
            if (!pvpEventsRegistered) return;
            if (PvPNetworkManager.Instance == null) return;

            PvPNetworkManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
            PvPNetworkManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
            PvPNetworkManager.Instance.OnConnectedToServerEvent -= OnNetworkConnected;
            PvPNetworkManager.Instance.OnShutdownEvent -= OnNetworkShutdown;
            PvPNetworkManager.Instance.OnLobbyRoleReceived -= OnOpponentRoleReceived;
            pvpEventsRegistered = false;
        }
        
        private void LoadOwnedRoles()
        {
            ownedRoleEntries = new List<RoleRegistry.RoleEntry>();
            localSelectedRole = null;
            opponentRoleName = null;

            PlayerDataManager pm = PlayerDataManager.Instance;
            RoleRegistry registry = GameDataManager.RoleRegistry;
            if (pm == null || registry == null)
            {
                Debug.LogWarning("[PvPLobbyPanel] Cannot load roles: PlayerDataManager or RoleRegistry is null");
                return;
            }

            foreach (KeyValuePair<RoleName, RoleSaveData> kv in pm.PlayerData.ownedRoles)
            {
                RoleRegistry.RoleEntry? entry = registry.GetRoleEntry(kv.Key);
                if (entry != null) ownedRoleEntries.Add(entry.Value);
            }

            if (ownedRoleEntries.Count == 0)
            {
                Debug.LogWarning("[PvPLobbyPanel] No owned roles available");
            }
        }

        /// <summary>
        /// 选角按钮的交互权限
        /// </summary>
        private void RefreshRoleSelectButtons()
        {
            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;
            roleASelectBtn.interactable = isHost;
            roleBSelectBtn.interactable = !isHost;
        }

        private void OnRoleASelectClicked()
        {
            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;
            if (!isHost || ownedRoleEntries.Count == 0) return;

            roleSelectPopupView.ShowForPvP(ownedRoleEntries, OnLocalRoleConfirmed);
        }

        private void OnRoleBSelectClicked()
        {
            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;
            if (isHost || ownedRoleEntries.Count == 0) return;

            roleSelectPopupView.ShowForPvP(ownedRoleEntries, OnLocalRoleConfirmed);
        }

        /// <summary>
        /// 本地玩家确认选角后的回调。更新 UI，同步数据给对手
        /// </summary>
        private void OnLocalRoleConfirmed(RoleRegistry.RoleEntry selected)
        {
            
            localSelectedRole = selected;
            LevelContext.PvPSelectedRole = selected;

            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;

            if (isHost)
            {
                if (playerARoleNameText != null) playerARoleNameText.text = selected.roleName.ToString();
                if (roleAImage != null) roleAImage.sprite = selected.template.selectedIcon;
            }
            else
            {
                if (playerBRoleNameText != null) playerBRoleNameText.text = selected.roleName.ToString();
                if (roleBImage != null) roleBImage.sprite = selected.template.selectedIcon;
            }
            
            PvPNetworkManager.Instance?.SendLobbyRole(selected.roleName.ToString());
            
            Debug.Log($"[PvPLobbyPanel] Local role selected: {selected.roleName}");

            CheckBothReady();
        }

        /// <summary>
        /// 收到对手的选角信息
        /// </summary>
        private void OnOpponentRoleReceived(PlayerRef player, string roleName)
        {
            opponentRoleName = roleName;

            if (!System.Enum.TryParse(roleName, out RoleName role))
            {
                Debug.LogWarning($"[PvPLobbyPanel] Unknown role name: {roleName}");
                return;
            }
            
            RoleRegistry.RoleEntry? entry = GameDataManager.RoleRegistry?.GetRoleEntry(role);
            if (entry == null) return;

            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;

            if (!isHost)
            {
                if (playerARoleNameText != null) playerARoleNameText.text = roleName;
                if (roleAImage != null) roleAImage.sprite = entry.Value.template.selectedIcon;
            }
            else
            {
                if (playerBRoleNameText != null) playerBRoleNameText.text = roleName;
                if (roleBImage != null) roleBImage.sprite = entry.Value.template.selectedIcon;
            }

            Debug.Log($"[PvPLobbyPanel] Opponent selected role: {roleName}");

            CheckBothReady();
        }

        /// <summary>
        /// 双方都选好角色后启用开始按钮
        /// </summary>
        private void CheckBothReady()
        {
            bool success = localSelectedRole != null && !string.IsNullOrEmpty(opponentRoleName);
            startBattleButton.interactable = success;
            startBattleButton.gameObject.SetActive(success);
            if (success)
            {
                waitingStatusText.gameObject.SetActive(true);
                waitingStatusText.text = "双方已就绪，可以开始战斗！";
            }
        }
        
        private async void OnCreateRoomClicked()
        {
            createRoomButton.interactable = false; 
            joinRoomButton.interactable = false;
            roomPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            loadingCoin.SetActive(true);
            
            StartCoroutine(HideStatusTextAfterDelay(2.5f, "正在创建房间..."));

            if (PvPNetworkManager.Instance == null)
            {
                waitingStatusText.text = "网络连接超时，请重试。";
                return;
            }

            await PvPNetworkManager.Instance.CreateHost();

            if (PvPNetworkManager.Instance.Runner != null && PvPNetworkManager.Instance.Runner.IsRunning)
            {
                loadingCoin.SetActive(false);
                roomPanel.SetActive(true);
                roomCodeText.text = PvPNetworkManager.Instance.SessionName;
                waitingStatusText.text = "等待对手加入...";
                RefreshRoleSelectButtons();
            }
            else
            {
                loadingCoin.SetActive(false);
                StartCoroutine(HideStatusTextAfterDelay(2.5f, "创建房间失败，请重试"));
                
                createRoomButton.interactable = true;
                joinRoomButton.interactable = true;
            }
        }
        
        private void OnJoinRoomClicked()
        {
            roomPanel.SetActive(false);
            joinRoomPanel.SetActive(true);
            confirmJoinButton.gameObject.SetActive(true);
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
        }
        private async void OnConfirmJoinClicked()
        {
            string code = roomCodeInput.text?.Trim().ToUpper();
            waitingStatusText.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(code) || code.Length < 4)
            {
                StartCoroutine(HideStatusTextAfterDelay(2f, "请输入有效的房间码"));
                joinRoomPanel.SetActive(false);
                createRoomButton.interactable = true;
                joinRoomButton.interactable = true;
                return;
            }
            
            joinRoomPanel.SetActive(false);
            loadingCoin.SetActive(true);
            StartCoroutine(HideStatusTextAfterDelay(1f, "正在加入房间..."));

            if (PvPNetworkManager.Instance == null)
            {
                StartCoroutine(HideStatusTextAfterDelay(1f, "网络连接超时，请重试"));
                return;
            }

            await PvPNetworkManager.Instance.CreateClient(code);

            if (PvPNetworkManager.Instance.Runner != null && PvPNetworkManager.Instance.Runner.IsRunning)
            {
                loadingCoin.SetActive(false);
                waitingStatusText.gameObject.SetActive(false);

                roomPanel.SetActive(true);
                roomCodeText.text = PvPNetworkManager.Instance.SessionName;

                RefreshRoleSelectButtons();
            }
            else
            {
                StartCoroutine(HideStatusTextAfterDelay(1f, "加入房间失败，请检查房间码"));
                joinRoomPanel.SetActive(true);
            }
        }
        
        private IEnumerator HideStatusTextAfterDelay(float delay, string message)
        {
            waitingStatusText.gameObject.SetActive(true);
            waitingStatusText.text = message;
            
            yield return new WaitForSeconds(delay);
            waitingStatusText.gameObject.SetActive(false);
        }
        
        private void OnPlayerJoined(PlayerRef player)
        {
            if (PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.PlayerCount >= 2)
            {
                StartCoroutine(HideStatusTextAfterDelay(2.5f, "请双方选择英雄"));
                
                if (localSelectedRole != null)
                {
                    PvPNetworkManager.Instance?.SendLobbyRole(localSelectedRole.Value.roleName.ToString());
                    Debug.Log($"[PvPLobbyPanel] Resent local role to newly joined player: {player.PlayerId}");
                }
            }
        }

        private void OnPlayerLeft(PlayerRef player)
        {
            opponentRoleName = null;
            
            bool isHost = PvPNetworkManager.Instance != null && PvPNetworkManager.Instance.IsHost;
            if (isHost)
            {
                if (playerBRoleNameText != null) playerBRoleNameText.text = "";
                if (roleBImage != null) roleBImage.sprite = defaultSprite;
            }
            else
            {
                if (playerARoleNameText != null) playerARoleNameText.text = "";
                if (roleAImage != null) roleAImage.sprite = defaultSprite;
            }

            startBattleButton.gameObject.SetActive(false);
            startBattleButton.interactable = false;
            StartCoroutine(HideStatusTextAfterDelay(3f, "对手已离开，等待新对手..."));
        }

        private void OnNetworkConnected()
        {
            Debug.Log("[PvPLobbyPanel] Network connected.");
        }

        private void OnNetworkShutdown()
        {
            Hide();
            ResetUI();
            Debug.Log("[PvPLobbyPanel] Network shutdown, returning to menu.");
        }
        
        private void OnStartBattleClicked()
        {
            if (localSelectedRole == null) return;

            LevelContext.PvPSelectedRole = localSelectedRole;
            LevelContext.IsPvPMode = true;  

            panelRoot.SetActive(false);
            
            NetworkRunner runner = PvPNetworkManager.Instance?.Runner;
            if (runner != null && runner.IsRunning)
            {
                SceneRef sceneRef = PvPNetworkManager.Instance.PvpSceneRef;
                Debug.Log($"[PvPLobbyPanel] Loading PvP scene via Fusion: scene index={sceneRef}");
                runner.LoadScene(sceneRef);
            }
            else
            {
                Debug.LogError("[PvPLobbyPanel] Runner is not running, cannot load PvP scene.");
            }
        }
    }
}
