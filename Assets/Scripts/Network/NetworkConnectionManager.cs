using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Network
{
    public class NetworkConnectionManager : MonoBehaviour
    {
        private string playerName;
        private float lobbyPollTimer;

        public static NetworkConnectionManager Instance { get; private set; }
        public Lobby JoinedLobby { get; private set; }
        public event Action<Lobby> OnJoinedLobby;
        public event Action<Lobby> OnJoinedLobbyUpdate;
        public List<Lobby> LobbiesList { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            HandleLobbyPolling();
        }

        public async Task InitializeAsync(string playerName)
        {
            Debug.Log($"[NetworkConnectionManager] Initializing Unity Services for player: {playerName}");

            try
            {
                // Проверяем, не инициализированы ли уже сервисы
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    InitializationOptions initializationOptions = new InitializationOptions();
                    initializationOptions.SetProfile(playerName);

                    await UnityServices.InitializeAsync(initializationOptions);
                    Debug.Log("[NetworkConnectionManager] Unity Services initialized");
                }
                else
                {
                    Debug.Log("[NetworkConnectionManager] Unity Services already initialized");
                }

                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log($"[NetworkConnectionManager] Player signed in with id: {AuthenticationService.Instance.PlayerId}");
                };

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("[NetworkConnectionManager] Signing in anonymously...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log("[NetworkConnectionManager] Signed in successfully");
                }
                else
                {
                    Debug.Log("[NetworkConnectionManager] Already signed in");
                }

                this.playerName = playerName;
                Debug.Log($"[NetworkConnectionManager] Initialization complete for player: {playerName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkConnectionManager] Failed to initialize: {e.Message}");
                // Log inner exceptions for Unity Services initialization failures
                var inner = e.InnerException;
                while (inner != null)
                {
                    Debug.LogError($"[NetworkConnectionManager] Inner exception: {inner.Message}");
                    inner = inner.InnerException;
                }
                throw; // Перебрасываем исключение чтобы вызывающий код мог обработать
            }
        }

        public async Task CreateLobbyAndHost(string name, int maxPlsyers)
        {
            try
            {
                Player player = GetPlayer();

                // Получить локальный IP и порт
                string localIP = NetworkIPHelper.GetLocalIPAddress();
                int port = NetworkIPHelper.GetDefaultPort();

                // Сохранить IP:Port в Lobby Data
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    Player = player,
                    Data = new Dictionary<string, DataObject> {
                        { "HostIP", new DataObject(DataObject.VisibilityOptions.Public, localIP) },
                        { "HostPort", new DataObject(DataObject.VisibilityOptions.Public, port.ToString()) }
                    }
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlsyers, options);
                JoinedLobby = lobby;
                OnJoinedLobby?.Invoke(JoinedLobby);

                // Heartbeat Loop (Simple Implementation)
                StartCoroutine(HeartbeatLobby(lobby.Id, 15f));

                Debug.Log($"Host started. Lobby Code: {lobby.LobbyCode}. IP: {localIP}:{port}");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private Player GetPlayer()
        {
            return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
        });
        }

        public async Task QueryLobbies()
        {
            try
            {
                LobbiesList = new List<Lobby>();

                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
                
                if (queryResponse.Results.Count == 0)
                {
                    Debug.Log("No lobbies found.");
                    return;
                }
                LobbiesList = queryResponse.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        private async void HandleLobbyPolling()
        {
            if (JoinedLobby == null) return;

            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(JoinedLobby);
            }
        }

        public async Task JoinLobby(string id)
        {
            try
            {
                Player player = GetPlayer();

                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = player,
                };
                Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, options);
                JoinedLobby = lobby;
                OnJoinedLobby?.Invoke(JoinedLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        public async void LeaveLobby()
        {
            if (JoinedLobby == null) return;
            
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

                JoinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
           
        }

        private System.Collections.IEnumerator HeartbeatLobby(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }

        /// <summary>
        /// Получает IP адрес хоста из Lobby Data
        /// </summary>
        public string GetHostIPFromLobby()
        {
            if (JoinedLobby?.Data != null && JoinedLobby.Data.ContainsKey("HostIP"))
            {
                return JoinedLobby.Data["HostIP"].Value;
            }
            return null;
        }

        /// <summary>
        /// Получает порт хоста из Lobby Data
        /// </summary>
        public int GetHostPortFromLobby()
        {
            if (JoinedLobby?.Data != null && JoinedLobby.Data.ContainsKey("HostPort"))
            {
                if (int.TryParse(JoinedLobby.Data["HostPort"].Value, out int port))
                {
                    return port;
                }
            }
            return NetworkIPHelper.GetDefaultPort();
        }

        /// <summary>
        /// Обновляет Lobby Data с флагом GameStarted
        /// </summary>
        public async Task UpdateLobbyGameStarted(bool started)
        {
            if (JoinedLobby == null) return;

            try
            {
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> {
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, started.ToString()) }
                    }
                };
                JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, updateOptions);
                Debug.Log($"Lobby GameStarted flag updated to: {started}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to update GameStarted flag: {e}");
            }
        }

        /// <summary>
        /// Обновляет IP и порт хоста в Lobby Data
        /// </summary>
        public async Task UpdateLobbyHostIP(string ip, int port)
        {
            if (JoinedLobby == null) return;

            try
            {
                var updateOptions = new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> {
                        { "HostIP", new DataObject(DataObject.VisibilityOptions.Public, ip) },
                        { "HostPort", new DataObject(DataObject.VisibilityOptions.Public, port.ToString()) }
                    }
                };
                JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, updateOptions);
                Debug.Log($"Lobby HostIP updated to: {ip}:{port}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to update HostIP: {e}");
            }
        }

        /// <summary>
        /// Проверяет, является ли текущий игрок хостом лобби
        /// </summary>
        public bool IsHost()
        {
            if (JoinedLobby == null) return false;
            return JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        /// <summary>
        /// Проверяет, началась ли игра (по флагу GameStarted в Lobby Data)
        /// </summary>
        public bool IsGameStarted()
        {
            if (JoinedLobby?.Data != null && JoinedLobby.Data.ContainsKey("GameStarted"))
            {
                return bool.TryParse(JoinedLobby.Data["GameStarted"].Value, out bool started) && started;
            }
            return false;
        }
    }
}
