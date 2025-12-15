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
            Debug.Log("Initializing Unity Services");
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log("Player id: " + AuthenticationService.Instance.PlayerId);
            };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            this.playerName = playerName;
        }

        public async Task CreateLobbyAndHost(string name, int maxPlsyers)
        {
            try
            {
                Player player = GetPlayer();

                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    Player = player,
                    Data = new Dictionary<string, DataObject> {}
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlsyers, options);
                JoinedLobby = lobby;
                OnJoinedLobby?.Invoke(JoinedLobby);

                // Heartbeat Loop (Simple Implementation)
                StartCoroutine(HeartbeatLobby(lobby.Id, 15f));

                Debug.Log($"Host started. Lobby Code: {lobby.LobbyCode}.");
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
    }
}
