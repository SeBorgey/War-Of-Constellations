using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Network
{
    public class NetworkConnectionManager : MonoBehaviour
    {
        public static NetworkConnectionManager Instance { get; private set; }

        private const string JoinCodeKey = "j";

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

        public async Task InitializeAsync()
        {
            Debug.Log("Initializing Unity Services");
            await UnityServices.InitializeAsync();
            
            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log("Player id: " + AuthenticationService.Instance.PlayerId);
            };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

        }

        public async Task CreateLobbyAndHost(string name, int maxPlsyers)
        {
            try
            {
                // 1. Create Relay
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                RelayServerData releyServerData = new RelayServerData(allocation, "dtls");
                // 2. Setup Transport
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(releyServerData);

                // 3. Create Lobby
                var options = new CreateLobbyOptions();
                options.Data = new Dictionary<string, DataObject>
                {
                    { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlsyers, options);
                
                // Heartbeat Loop (Simple Implementation)
                StartCoroutine(HeartbeatLobby(lobby.Id, 15f));

                // 4. Start Host
                NetworkManager.Singleton.StartHost();
                Debug.Log($"Host started. Lobby Code: {lobby.LobbyCode}. Relay Code: {joinCode}");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
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

        public async Task JoinRealey(string joinCode)
        {
            try
            {
                // 1. Join Relay
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData releyServerData = new RelayServerData(joinAllocation, "dtls");

                // 2. Setup Transport
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(releyServerData);

                // 3. Start Client
                NetworkManager.Singleton.StartClient();
                Debug.Log("Joined Lobby and started Client.");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        public async Task JoinLobby(string id)
        {
            try
            {
                await Lobbies.Instance.JoinLobbyByIdAsync(id);
                //NetworkManager.Singleton.StartClient();
                Debug.Log("Joined Lobby");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
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
