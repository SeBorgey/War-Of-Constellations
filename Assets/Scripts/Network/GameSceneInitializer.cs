using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Network
{
    /// <summary>
    /// Инициализирует сетевую сессию при загрузке сцены Game
    /// </summary>
    public class GameSceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[GameSceneInitializer] Starting initialization...");

            // Проверяем наличие NetworkManager в сцене
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkManager.Singleton is null! Make sure NetworkManager exists in the Game scene.");
                return;
            }

            Debug.Log("[GameSceneInitializer] NetworkManager found in scene");

            // Создаем NetworkGameManager как компонент на существующем NetworkManager
            if (NetworkGameManager.Instance == null)
            {
                var gameManager = networkManager.GetComponent<NetworkGameManager>();
                if (gameManager == null)
                {
                    Debug.Log("[GameSceneInitializer] Adding NetworkGameManager component to NetworkManager");
                    gameManager = networkManager.gameObject.AddComponent<NetworkGameManager>();
                }
                else
                {
                    Debug.Log("[GameSceneInitializer] NetworkGameManager component already exists on NetworkManager");
                }
            }
            else
            {
                Debug.Log("[GameSceneInitializer] NetworkGameManager.Instance already exists");
            }

            // Инициализируем сетевую сессию
            InitializeNetworkSession();
        }

        private void InitializeNetworkSession()
        {
            Debug.Log("[GameSceneInitializer] Initializing network session...");

            var connectionManager = NetworkConnectionManager.Instance;
            if (connectionManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkConnectionManager.Instance is null!");
                return;
            }

            Debug.Log("[GameSceneInitializer] NetworkConnectionManager found");

            var gameManager = NetworkGameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkGameManager.Instance is null!");
                return;
            }

            Debug.Log("[GameSceneInitializer] NetworkGameManager found");

            // Проверяем, является ли игрок хостом
            bool isHost = connectionManager.IsHost();
            Debug.Log($"[GameSceneInitializer] IsHost: {isHost}");

            if (isHost)
            {
                // Хост: получаем IP и порт, запускаем Host
                string ip = NetworkIPHelper.GetLocalIPAddress();
                int port = NetworkIPHelper.GetDefaultPort();

                Debug.Log($"[GameSceneInitializer] Starting Host on {ip}:{port}");
                bool started = gameManager.StartHost(ip, port);
                if (!started)
                {
                    Debug.LogError("[GameSceneInitializer] Failed to start Host!");
                }
                else
                {
                    Debug.Log("[GameSceneInitializer] Host started successfully");
                }
            }
            else
            {
                // Клиент: получаем IP и порт из Lobby, подключаемся
                Debug.Log("[GameSceneInitializer] Initializing as Client...");
                TryConnectAsClient(connectionManager, gameManager);
            }
        }

        private void TryConnectAsClient(NetworkConnectionManager connectionManager, NetworkGameManager gameManager)
        {
            string hostIP = connectionManager.GetHostIPFromLobby();
            int hostPort = connectionManager.GetHostPortFromLobby();

            Debug.Log($"[GameSceneInitializer] HostIP from Lobby: {hostIP}, Port: {hostPort}");

            if (string.IsNullOrEmpty(hostIP))
            {
                Debug.LogWarning("[GameSceneInitializer] HostIP not found in Lobby Data. Will retry connection in 1 second...");
                // Повторяем попытку подключения через небольшую задержку
                Invoke(nameof(RetryClientConnection), 1f);
                return;
            }

            Debug.Log($"[GameSceneInitializer] Connecting as Client to {hostIP}:{hostPort}");
            bool started = gameManager.StartClient(hostIP, hostPort);
            if (!started)
            {
                Debug.LogError($"[GameSceneInitializer] Failed to start Client connecting to {hostIP}:{hostPort}! Will retry in 2 seconds...");
                // Повторяем попытку подключения через небольшую задержку
                Invoke(nameof(RetryClientConnection), 2f);
            }
            else
            {
                Debug.Log("[GameSceneInitializer] Client connection initiated successfully");
            }
        }

        private void RetryClientConnection()
        {
            Debug.Log("[GameSceneInitializer] Retrying client connection...");

            var connectionManager = NetworkConnectionManager.Instance;
            var gameManager = NetworkGameManager.Instance;

            if (connectionManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkConnectionManager.Instance is null during retry!");
                return;
            }

            if (gameManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkGameManager.Instance is null during retry!");
                return;
            }

            // Проверяем, не подключились ли мы уже
            if (gameManager.IsClient || gameManager.IsServer)
            {
                Debug.Log("[GameSceneInitializer] Already connected, skipping retry");
                return;
            }

            TryConnectAsClient(connectionManager, gameManager);
        }
    }
}

