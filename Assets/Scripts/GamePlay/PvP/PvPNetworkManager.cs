using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using GamePlay.GameModel.Level;
using GamePlay.PlayerDataHandle;
using UnityEngine;

namespace GamePlay.PvP
{
    public class PvPNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static PvPNetworkManager Instance { get; private set; }

        [SerializeField] private NetworkRunner runnerPrefab;

        /// <summary> PvP 战斗场景在 Build Settings 中的索引 </summary>
        [SerializeField] private int pvpSceneBuildIndex = 2;

        /// <summary> PvP 战斗场景的 Fusion SceneRef </summary>
        public SceneRef PvpSceneRef => SceneRef.FromIndex(pvpSceneBuildIndex);

        /// <summary> 大厅选角同步 key </summary>
        private static readonly ReliableKey LobbyRoleKey = ReliableKey.FromInts(1, 0);

        private NetworkRunner runner;
        public NetworkRunner Runner => runner;

        public string SessionName { get; private set; }
        public bool IsHost { get; private set; }
        public int PlayerCount { get; private set; }
        public PlayerRef LocalPlayer { get; private set; }
        public PlayerRef RemotePlayer { get; private set; }
        
        private string pendingLocalRole;
        
        public event Action<PlayerRef> OnPlayerJoinedEvent;
        public event Action<PlayerRef> OnPlayerLeftEvent;
        public event Action OnShutdownEvent;
        public event Action OnConnectedToServerEvent;
        public event Action<NetConnectFailedReason> OnConnectFailedEvent;
        /// <summary> 大厅中对手发来的角色选择 (player, roleName) </summary>
        public event Action<PlayerRef, string> OnLobbyRoleReceived;
        /// <summary> PvP 战斗场景在所有端加载完毕 </summary>
        public event Action OnPvPSceneLoaded;
        /// <summary> 收到对方返回大厅的请求 </summary>
        public event Action OnReturnToLobbyRequested;

        private bool manualShutdown;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public async Task CreateHost(string sessionName = null)
        {
            IsHost = true;
            SessionName = sessionName ?? GenerateRoomCode();
            await StartRunner(GameMode.Host);
        }
        
        public async Task CreateClient(string sessionName)
        {
            IsHost = false;
            SessionName = sessionName;
            await StartRunner(GameMode.Client);
        }
        
        private async Task StartRunner(GameMode mode)
        {
            if (runner != null && runner.IsRunning)
            {
                Debug.LogWarning("[PvPNetworkManager] Runner is already running, shutting down first.");
                await runner.Shutdown();
            }

            runner = Instantiate(runnerPrefab);
            runner.name = mode.ToString();
            DontDestroyOnLoad(runner.gameObject);

            if (runner.GetComponent<INetworkSceneManager>() == null) runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            if (runner.GetComponent<INetworkObjectProvider>() == null) runner.gameObject.AddComponent<NetworkObjectProviderDefault>();

            runner.AddCallbacks(this);

            StartGameArgs args = new ()
            {
                GameMode = mode,
                SessionName = SessionName,
                PlayerCount = 2,
                SceneManager = runner.GetComponent<INetworkSceneManager>(),
                ObjectProvider = runner.GetComponent<INetworkObjectProvider>(),
            };

            Debug.Log($"[PvPNetworkManager] Starting {mode} with session: {SessionName}");

            StartGameResult result = await runner.StartGame(args);

            if (result.Ok)
            {
                LocalPlayer = runner.LocalPlayer;
                Debug.Log($"[PvPNetworkManager] Runner started OK. LocalPlayer: {LocalPlayer}");
            }
            else
            {
                Debug.LogError($"[PvPNetworkManager] Failed to start runner: {result.ShutdownReason}");
                OnConnectFailedEvent?.Invoke(NetConnectFailedReason.Timeout);
            }
        }

        /// <summary>
        /// 发送本地选角给对方
        /// </summary>
        public bool SendLobbyRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return false;
            
            pendingLocalRole = roleName;
            if (runner == null || !runner.IsRunning)
            {
                Debug.LogWarning("[PvPNetworkManager] Cannot send lobby role: runner not running");
                return false;
            }

            if (IsHost && RemotePlayer == default)
            {
                Debug.Log("[PvPNetworkManager] Opponent not joined yet, role cached locally, will send on join.");
                return false;
            }
            byte[] data = System.Text.Encoding.UTF8.GetBytes(roleName);

            if (IsHost)
            {
                runner.SendReliableDataToPlayer(RemotePlayer, LobbyRoleKey, data);
            }
            else
            {
                runner.SendReliableDataToServer(LobbyRoleKey, data);
            }
            
            Debug.Log($"[PvPNetworkManager] Sent lobby role: {roleName}");
            return true;
        }

        public void Shutdown()
        {
            // 主动关闭：OnShutdown 回调不再触发 OnShutdownEvent，避免大厅面板被 Hide
            manualShutdown = true;

            if (runner != null && runner.IsRunning) runner.Shutdown();
            if (runner != null) Destroy(runner.gameObject);

            runner = null;
            IsHost = false;
            PlayerCount = 0;
            SessionName = null;
            LocalPlayer = default;
            RemotePlayer = default;
        }

        private static string GenerateRoomCode()
        {
            const string str = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] code = new char[6];
            for (int i = 0; i < 6; i++)
            {
                code[i] = str[UnityEngine.Random.Range(0, str.Length)];
            }
            return new string(code);
        }
        

        public void OnPlayerJoined(NetworkRunner _runner, PlayerRef player)
        {
            PlayerCount++;
            if (player != LocalPlayer) RemotePlayer = player;
            OnPlayerJoinedEvent?.Invoke(player);
            SendLobbyRole(pendingLocalRole);
            Debug.Log($"[PvPNetworkManager] Player {player.PlayerId} joined. Total: {PlayerCount}");
        }

        public void OnPlayerLeft(NetworkRunner _runner, PlayerRef player)
        {
            PlayerCount--;
            if (player == RemotePlayer) RemotePlayer = default;
            OnPlayerLeftEvent?.Invoke(player);
            Debug.Log($"[PvPNetworkManager] Player {player.PlayerId} left. Total: {PlayerCount}");

            // 对战场景中对方离开（返回大厅/退出/断线）：本端也应返回大厅。
            // 可靠消息可能在对方 Shutdown 前未发出，断线回调是兜底同步机制。
            if (LevelContext.IsPvPMode) OnReturnToLobbyRequested?.Invoke();
        }

        public void OnShutdown(NetworkRunner _runner, ShutdownReason shutdownReason)
        {
            // 主动返回大厅触发的关闭：不通知订阅者（PvPLobbyPanel 会 Hide 大厅面板）
            if (manualShutdown)
            {
                manualShutdown = false;
                Debug.Log($"[PvPNetworkManager] Runner shutdown (manual): {shutdownReason}");
                return;
            }

            OnShutdownEvent?.Invoke();
            Debug.Log($"[PvPNetworkManager] Runner shutdown: {shutdownReason}");

            // 对战场景中连接被关闭（host 返回大厅/退出/断线）：本端也应返回大厅。
            // 可靠消息可能因对方立即 Shutdown 而丢失，断线回调是兜底同步机制。
            if (LevelContext.IsPvPMode) OnReturnToLobbyRequested?.Invoke();
        }

        public void OnConnectedToServer(NetworkRunner _runner)
        {
            OnConnectedToServerEvent?.Invoke();
            Debug.Log("[PvPNetworkManager] Connected to server.");
        }
        
        public void OnInput(NetworkRunner _runner, NetworkInput input)
        {
            PlayerInputHandler.PvPProvider?.OnFusionInput(input);
        }
        
        public void OnDisconnectedFromServer(NetworkRunner _runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner _runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { request.Accept(); }
        public void OnConnectFailed(NetworkRunner _runner, NetAddress remoteAddress, NetConnectFailedReason reason) { OnConnectFailedEvent?.Invoke(reason); }
        public void OnInputMissing(NetworkRunner _runner, PlayerRef player, NetworkInput input) { }
        public void OnUserSimulationMessage(NetworkRunner _runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner _runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            if (key == LobbyRoleKey)
            {
                if (data.Array != null)
                {
                    string roleName = System.Text.Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);
                    Debug.Log($"[PvPNetworkManager] Received lobby role from {player.PlayerId}: {roleName}");
                    OnLobbyRoleReceived?.Invoke(player, roleName);
                }
            }
        }
        public void OnReliableDataProgress(NetworkRunner _runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnObjectExitAOI(NetworkRunner _runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner _runner, NetworkObject obj, PlayerRef player) { }
        public void OnSessionListUpdated(NetworkRunner _runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner _runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner _runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner _runner)
        {
            Debug.Log("[PvPNetworkManager] Scene load done — all peers have loaded the scene.");
            OnPvPSceneLoaded?.Invoke();
        }
        public void OnSceneLoadStart(NetworkRunner _runner)
        {
            Debug.Log("[PvPNetworkManager] Scene load started.");
        }
    }
}
