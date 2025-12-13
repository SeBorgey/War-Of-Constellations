using UnityEngine;

namespace Network
{
    // Stub for Network interactions (Netcode for GameObjects)
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

        public void StartHost()
        {
            Debug.Log("Starting Host...");
            // NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            Debug.Log("Starting Client...");
            // NetworkManager.Singleton.StartClient();
        }
    }
}
