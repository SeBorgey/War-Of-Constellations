using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network
{
    public class NetworkGameManager : MonoBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }

        private NetworkManager _networkManager;
        private UnityTransport _transport;

        public event Action<ulong> OnPlayerConnected;
        public event Action<ulong> OnPlayerDisconnected;
        public event Action OnServerStarted;
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;

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
                return;
            }

            _networkManager = NetworkManager.Singleton;
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager.Singleton is null! Make sure NetworkManager exists in the scene.");
                return;
            }

            _transport = _networkManager.GetComponent<UnityTransport>();
            if (_transport == null)
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
            }
        }

        private void OnEnable()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnectedCallback;
                _networkManager.OnServerStarted += OnServerStartedCallback;
                
                // Обработка успешного подключения клиента на стороне клиента
                if (_networkManager.IsClient && !_networkManager.IsServer)
                {
                    // Клиент успешно подключился
                    OnClientConnected?.Invoke();
                }
            }
        }

        private void OnDisable()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
                _networkManager.OnServerStarted -= OnServerStartedCallback;
            }
        }

        /// <summary>
        /// Запускает Netcode Host на указанном IP и порту
        /// </summary>
        public bool StartHost(string ip, int port)
        {
            Debug.Log($"[NetworkGameManager] StartHost called with IP: {ip}, Port: {port}");

            if (_networkManager == null)
            {
                Debug.LogError("[NetworkGameManager] NetworkManager is not initialized!");
                return false;
            }

            if (_transport == null)
            {
                Debug.LogError("[NetworkGameManager] UnityTransport is not initialized!");
                return false;
            }

            // Проверяем, что Netcode еще не запущен
            if (_networkManager.IsServer || _networkManager.IsClient)
            {
                Debug.LogWarning("[NetworkGameManager] NetworkManager is already running. Shutting down first.");
                try
                {
                    _networkManager.Shutdown();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[NetworkGameManager] Error during shutdown: {e.Message}");
                }
            }

            // Валидация IP и порта
            if (string.IsNullOrEmpty(ip))
            {
                Debug.LogError("[NetworkGameManager] IP address is null or empty!");
                return false;
            }

            if (port < 1 || port > 65535)
            {
                Debug.LogError($"[NetworkGameManager] Invalid port: {port}. Port must be between 1 and 65535.");
                return false;
            }

            try
            {
                // Настраиваем UnityTransport
                _transport.SetConnectionData(ip, (ushort)port);
                Debug.Log($"[NetworkGameManager] UnityTransport configured with {ip}:{port}");

                // Запускаем Host
                bool started = _networkManager.StartHost();
                if (started)
                {
                    Debug.Log($"[NetworkGameManager] Host started successfully on {ip}:{port}");
                }
                else
                {
                    Debug.LogError($"[NetworkGameManager] Failed to start Host on {ip}:{port}. Check logs for details.");
                }

                return started;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NetworkGameManager] Exception while starting Host: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Запускает Netcode Client и подключается к указанному IP и порту
        /// </summary>
        public bool StartClient(string ip, int port)
        {
            Debug.Log($"[NetworkGameManager] StartClient called with IP: {ip}, Port: {port}");

            if (_networkManager == null)
            {
                Debug.LogError("[NetworkGameManager] NetworkManager is not initialized!");
                return false;
            }

            if (_transport == null)
            {
                Debug.LogError("[NetworkGameManager] UnityTransport is not initialized!");
                return false;
            }

            // Проверяем, что Netcode еще не запущен
            if (_networkManager.IsServer || _networkManager.IsClient)
            {
                Debug.LogWarning("[NetworkGameManager] NetworkManager is already running. Shutting down first.");
                try
                {
                    _networkManager.Shutdown();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[NetworkGameManager] Error during shutdown: {e.Message}");
                }
            }

            // Валидация IP и порта
            if (string.IsNullOrEmpty(ip))
            {
                Debug.LogError("[NetworkGameManager] IP address is null or empty!");
                return false;
            }

            if (port < 1 || port > 65535)
            {
                Debug.LogError($"[NetworkGameManager] Invalid port: {port}. Port must be between 1 and 65535.");
                return false;
            }

            try
            {
                // Настраиваем UnityTransport
                _transport.SetConnectionData(ip, (ushort)port);
                Debug.Log($"[NetworkGameManager] UnityTransport configured with {ip}:{port}");

                // Запускаем Client
                bool started = _networkManager.StartClient();
                if (started)
                {
                    Debug.Log($"[NetworkGameManager] Client connection initiated to {ip}:{port}");
                }
                else
                {
                    Debug.LogError($"[NetworkGameManager] Failed to start Client connecting to {ip}:{port}. Check logs for details.");
                }

                return started;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NetworkGameManager] Exception while starting Client: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Останавливает сетевую сессию
        /// </summary>
        public void Shutdown()
        {
            if (_networkManager != null && (_networkManager.IsServer || _networkManager.IsClient))
            {
                _networkManager.Shutdown();
                Debug.Log("Network session shut down");
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            Debug.Log($"[NetworkGameManager] Client {clientId} connected");
            OnPlayerConnected?.Invoke(clientId);
            
            // Если это локальный клиент подключился к серверу
            if (_networkManager != null && _networkManager.LocalClientId == clientId && !_networkManager.IsServer)
            {
                Debug.Log($"[NetworkGameManager] Local client {clientId} successfully connected to server");
                OnClientConnected?.Invoke();
            }
        }

        private void OnClientDisconnectedCallback(ulong clientId)
        {
            Debug.LogWarning($"[NetworkGameManager] Client {clientId} disconnected");
            OnPlayerDisconnected?.Invoke(clientId);
        }

        private void OnServerStartedCallback()
        {
            Debug.Log("[NetworkGameManager] Server started successfully");
            OnServerStarted?.Invoke();
        }

        /// <summary>
        /// Проверяет, является ли текущий экземпляр сервером
        /// </summary>
        public bool IsServer => _networkManager != null && _networkManager.IsServer;

        /// <summary>
        /// Проверяет, является ли текущий экземпляр клиентом
        /// </summary>
        public bool IsClient => _networkManager != null && _networkManager.IsClient;

        /// <summary>
        /// Проверяет, является ли текущий экземпляр хостом (сервер + клиент)
        /// </summary>
        public bool IsHost => _networkManager != null && _networkManager.IsHost;
    }
}

