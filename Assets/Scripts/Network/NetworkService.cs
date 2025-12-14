using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkService : MonoBehaviour
    {
        public static NetworkService Instance { get; private set; }

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

        private async void Start()
        {
            await NetworkConnectionManager.Instance.InitializeAsync();
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();


        //    await NetworkConnectionManager.Instance.CreateLobbyAndHost();
        //}
        //        if (GUILayout.Button("Join Lobby (Relay)"))
        //        {
        //            await NetworkConnectionManager.Instance.JoinLobby();
        //
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
