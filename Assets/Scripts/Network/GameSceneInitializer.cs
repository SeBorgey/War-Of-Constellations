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
        // private void Awake()
        // {
        //     Debug.Log("[GameSceneInitializer] Awake called");
        // }

        private void Awake()
        {
            Debug.Log("[GameSceneInitializer] Start called");

            // Диагностика: ищем все NetworkManager в сцене
            var allManagers = FindObjectsByType<Unity.Netcode.NetworkManager>(FindObjectsSortMode.None);
            Debug.Log($"[GameSceneInitializer] Found {allManagers.Length} NetworkManager(s) in scene");
            foreach (var nm in allManagers)
            {
                Debug.Log($"[GameSceneInitializer] NM: {nm.gameObject.name}, scene={nm.gameObject.scene.name}, DontDestroyOnLoad={(nm.gameObject.scene.name == "DontDestroyOnLoad")}");
            }

            // NetworkManager должен уже существовать из MainMenu (DontDestroyOnLoad)
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null)
            {
                Debug.LogError("[GameSceneInitializer] NetworkManager.Singleton is null! NetworkManager was destroyed during scene change.");
                Debug.LogError("[GameSceneInitializer] Attempting to find NetworkManager in scene...");
                
                // Попытка найти NetworkManager в текущей сцене
                var managersInScene = FindObjectsByType<Unity.Netcode.NetworkManager>(FindObjectsSortMode.None);
                if (managersInScene.Length > 0)
                {
                    Debug.LogWarning($"[GameSceneInitializer] Found {managersInScene.Length} NetworkManager(s) in scene, but Singleton is null. This indicates a configuration issue.");
                    Debug.LogWarning("[GameSceneInitializer] Make sure Unity NetworkManager component has 'Dont Destroy' enabled in inspector.");
                }
                else
                {
                    Debug.LogError("[GameSceneInitializer] No NetworkManager found in scene! NetworkManager must exist before Game scene loads.");
                }
                return;
            }

            // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Убеждаемся, что NetworkManager не уничтожается при смене сцены
            if (networkManager.gameObject.scene.name != "DontDestroyOnLoad")
            {
                Debug.LogWarning("[GameSceneInitializer] NetworkManager is not in DontDestroyOnLoad scene! Setting DontDestroyOnLoad...");
                DontDestroyOnLoad(networkManager.gameObject);
            }

            Debug.Log($"[GameSceneInitializer] NetworkManager.Singleton found: {networkManager.gameObject.name}, IsServer={networkManager.IsServer}, IsClient={networkManager.IsClient}");

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

