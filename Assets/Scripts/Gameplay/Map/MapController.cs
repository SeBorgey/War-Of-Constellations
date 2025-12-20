using Unity.Netcode;
using UnityEngine;
using Gameplay.Map.Visualization;
using Network;

namespace Gameplay.Map
{
    /// <summary>
    /// Controls map generation and visualization initialization.
    /// Subscribes to network events to ensure proper timing.
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;
        [SerializeField] private MapVisualizer _mapVisualizer;
        [SerializeField] private GameMap _gameMap;

        private bool _mapGenerated = false;
        private bool _visualizationInitialized = false;

        private void Start()
        {
            // Проверяем, есть ли NetworkManager
            var networkManager = NetworkManager.Singleton;

            if (networkManager == null)
            {
                // Оффлайн режим (тесты)
                Debug.Log("[MapController] No NetworkManager - running in offline mode");
                GenerateMapOffline();
                return;
            }

            // Подписываемся на сетевые события
            SubscribeToNetworkEvents();

            // Проверяем, может уже подключены (если сцена загрузилась после StartHost/StartClient)
            if (networkManager.IsServer && networkManager.IsListening)
            {
                Debug.Log("[MapController] Already running as server, generating map");
                OnServerReady();
            }
            else if (networkManager.IsClient && networkManager.IsConnectedClient)
            {
                Debug.Log("[MapController] Already connected as client, waiting for stars");
                OnClientReady();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromNetworkEvents();
        }

        private void SubscribeToNetworkEvents()
        {
            var gameManager = NetworkGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnServerStarted += OnServerReady;
                gameManager.OnClientConnected += OnClientReady;
                Debug.Log("[MapController] Subscribed to network events");
            }
            else
            {
                Debug.LogWarning("[MapController] NetworkGameManager.Instance is null, will retry...");
                // NetworkGameManager может ещё не существовать, ждём
                StartCoroutine(WaitForNetworkGameManager());
            }
        }

        private void UnsubscribeFromNetworkEvents()
        {
            var gameManager = NetworkGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnServerStarted -= OnServerReady;
                gameManager.OnClientConnected -= OnClientReady;
            }
        }

        private System.Collections.IEnumerator WaitForNetworkGameManager()
        {
            float timeout = 15f;
            float elapsed = 0f;

            Debug.Log("[MapController] Waiting for NetworkGameManager...");

            while (NetworkGameManager.Instance == null && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.2f);
                elapsed += 0.2f;
            }

            if (NetworkGameManager.Instance != null)
            {
                Debug.Log($"[MapController] NetworkGameManager found after {elapsed:F1}s");
                SubscribeToNetworkEvents();

                // Ждём пока сеть будет готова
                yield return StartCoroutine(WaitForNetworkReady());
            }
            else
            {
                Debug.LogError("[MapController] Timeout waiting for NetworkGameManager!");
            }
        }

        private System.Collections.IEnumerator WaitForNetworkReady()
        {
            float timeout = 10f;
            float elapsed = 0f;

            var networkManager = NetworkManager.Singleton;

            while (networkManager != null && elapsed < timeout)
            {
                if (networkManager.IsServer && networkManager.IsListening && !_mapGenerated)
                {
                    Debug.Log("[MapController] Server is ready");
                    OnServerReady();
                    yield break;
                }
                else if (networkManager.IsClient && networkManager.IsConnectedClient && !_visualizationInitialized)
                {
                    Debug.Log("[MapController] Client is connected");
                    OnClientReady();
                    yield break;
                }

                yield return new WaitForSeconds(0.2f);
                elapsed += 0.2f;
            }

            Debug.LogWarning($"[MapController] Network not ready after {elapsed:F1}s. IsServer={networkManager?.IsServer}, IsClient={networkManager?.IsClient}, IsListening={networkManager?.IsListening}, IsConnectedClient={networkManager?.IsConnectedClient}");
        }

        /// <summary>
        /// Called when server/host is ready. Generates the map.
        /// </summary>
        private void OnServerReady()
        {
            if (_mapGenerated) return;
            _mapGenerated = true;

            Debug.Log("[MapController] Server ready - generating map");
            _mapGenerator?.GenerateMap();

            // Даём время на спавн NetworkObjects, затем инициализируем визуализацию
            Invoke(nameof(InitializeVisualizationDelayed), 0.2f);
        }

        /// <summary>
        /// Called when client successfully connects to server.
        /// </summary>
        private void OnClientReady()
        {
            if (_visualizationInitialized) return;

            Debug.Log("[MapController] Client connected - waiting for Star objects");
            StartCoroutine(WaitForStarsAndInitialize());
        }

        /// <summary>
        /// Offline mode - no networking.
        /// </summary>
        private void GenerateMapOffline()
        {
            Debug.Log("[MapController] Generating map in offline mode");
            _mapGenerator?.GenerateMap();
            _mapGenerated = true;
            InitializeVisualization();
        }

        private void InitializeVisualizationDelayed()
        {
            InitializeVisualization();
        }

        private void InitializeVisualization()
        {
            if (_visualizationInitialized) return;

            if (_mapVisualizer != null)
            {
                _mapVisualizer.Initialize();
                _visualizationInitialized = true;
                Debug.Log("[MapController] Visualization initialized");
            }
            else
            {
                Debug.LogWarning("[MapController] MapVisualizer reference not set!");
            }
        }

        /// <summary>
        /// Client waits for Star NetworkObjects to spawn, then initializes visualization.
        /// </summary>
        private System.Collections.IEnumerator WaitForStarsAndInitialize()
        {
            float timeout = 15f;
            float elapsed = 0f;

            // Ждём появления Star объектов (они NetworkBehaviour, синхронизируются автоматически)
            while (elapsed < timeout)
            {
                var stars = FindObjectsByType<Star>(FindObjectsSortMode.None);

                if (stars != null && stars.Length > 0)
                {
                    Debug.Log($"[MapController] Found {stars.Length} Star objects on client");

                    // Даём ещё немного времени на полную инициализацию
                    yield return new WaitForSeconds(0.3f);

                    // Инициализируем визуализацию на клиенте
                    InitializeClientVisualization(stars);
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }

            Debug.LogError("[MapController] Timeout waiting for Star objects on client!");
        }

        /// <summary>
        /// Initialize visualization on client using spawned Star objects.
        /// Client doesn't have GameMap/Constellation data, so we work directly with Stars.
        /// </summary>
        private void InitializeClientVisualization(Star[] stars)
        {
            if (_visualizationInitialized) return;
            _visualizationInitialized = true;

            Debug.Log($"[MapController] Initializing client visualization with {stars.Length} stars");

            // На клиенте просто добавляем StarView к каждой звезде
            // ConstellationView и MapEdgesView требуют данных о созвездиях,
            // которых нет на клиенте. Для полной визуализации нужно либо:
            // 1. Синхронизировать Constellation через NetworkVariable
            // 2. Передавать данные рёбер через RPC
            // Пока делаем только звёзды

            if (_mapVisualizer != null)
            {
                // Пытаемся стандартную инициализацию (если GameMap заполнен)
                if (_gameMap != null && _gameMap.Size() > 0)
                {
                    _mapVisualizer.Initialize();
                }
                else
                {
                    // Fallback: инициализируем только звёзды
                    InitializeStarsOnly(stars);
                }
            }
        }

        /// <summary>
        /// Fallback initialization for client - only star views, no edges.
        /// </summary>
        private void InitializeStarsOnly(Star[] stars)
        {
            Debug.Log("[MapController] Client fallback: initializing stars only (no edges)");

            // Получаем настройки визуализации
            var settings = GetVisualizationSettings();

            foreach (var star in stars)
            {
                if (star == null) continue;

                var existingView = star.GetComponent<StarView>();
                if (existingView == null)
                {
                    var starView = star.gameObject.AddComponent<StarView>();
                    starView.Initialize(settings);
                }
                else
                {
                    existingView.Initialize(settings);
                }
            }

            Debug.Log($"[MapController] Initialized {stars.Length} star views on client");
        }

        private MapVisualizationSettings GetVisualizationSettings()
        {
            if (_mapVisualizer != null && _mapVisualizer.Settings != null)
            {
                return _mapVisualizer.Settings;
            }
            return ScriptableObject.CreateInstance<MapVisualizationSettings>();
        }
    }
}
