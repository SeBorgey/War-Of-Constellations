using System.Collections.Generic;
using Network;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Gameplay.Mechanics;
using Gameplay.Map;

namespace UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI _goldText; // Общее поле (можно использовать для комбинированного отображения)
        [SerializeField] private TextMeshProUGUI _blueGoldText; // Золото Blue игрока
        [SerializeField] private TextMeshProUGUI _redGoldText; // Золото Red игрока
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerColorText; // Опционально: для отображения цвета игрока
        
        [Header("Network Info")]
        [SerializeField] private Transform _playersListContainer;
        [SerializeField] private GameObject _playerEntryPrefab;
        [SerializeField] private TextMeshProUGUI _connectionStatusText;

        private Dictionary<ulong, GameObject> _playerEntries = new Dictionary<ulong, GameObject>();
        private NetworkGameManager _networkGameManager;
        private Network.NetworkConnectionManager _connectionManager;
        private PlayerController _localPlayerController;
        private PlayerController _bluePlayerController;
        private PlayerController _redPlayerController;

        private bool _playerControllersInitialized = false;

        private void Start()
        {
            UpdateGold(0);
            InitializeNetworkTracking();
            // PlayerController инициализация перемещена в Update для ожидания их спавна
        }

        private void OnEnable()
        {
            if (_networkGameManager != null)
            {
                SubscribeToNetworkEvents();
            }
        }

        private void Update()
        {
            // Периодически пытаемся найти PlayerController, если они ещё не инициализированы
            if (!_playerControllersInitialized)
            {
                TryInitializePlayerControllers();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromNetworkEvents();
            UnsubscribeFromPlayerController();
        }

        private void OnDestroy()
        {
            UnsubscribeFromPlayerController();
        }

        private void TryInitializePlayerControllers()
        {
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsClient)
            {
                return;
            }

            // Ищем все PlayerController в сцене
            PlayerController[] allControllers = UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

            if (allControllers.Length == 0)
            {
                return; // Ещё нет контроллеров
            }

            bool foundBlue = false;
            bool foundRed = false;

            foreach (var controller in allControllers)
            {
                if (controller.PlayerColor == Player.Blue && _bluePlayerController == null)
                {
                    _bluePlayerController = controller;
                    _bluePlayerController.OnGoldChanged += OnBlueGoldChanged;
                    UpdateBlueGold(_bluePlayerController.Gold);
                    Debug.Log("[GameplayHUD] Found Blue PlayerController");
                    foundBlue = true;
                }
                else if (controller.PlayerColor == Player.Red && _redPlayerController == null)
                {
                    _redPlayerController = controller;
                    _redPlayerController.OnGoldChanged += OnRedGoldChanged;
                    UpdateRedGold(_redPlayerController.Gold);
                    Debug.Log("[GameplayHUD] Found Red PlayerController");
                    foundRed = true;
                }
            }

            // Определяем локального игрока на основе роли в сети
            if (_localPlayerController == null)
            {
                // Host = Blue, Client = Red
                _localPlayerController = networkManager.IsHost ? _bluePlayerController : _redPlayerController;

                if (_localPlayerController != null)
                {
                    UpdatePlayerColor(_localPlayerController.PlayerColor);
                    Debug.Log($"[GameplayHUD] Local player: {_localPlayerController.PlayerColor}");
                }
            }

            // Считаем инициализацию завершённой когда нашли хотя бы локального игрока
            if (_localPlayerController != null)
            {
                _playerControllersInitialized = true;
                Debug.Log("[GameplayHUD] PlayerControllers initialized successfully");

                // Обновляем комбинированное поле если используется
                if (_goldText != null && _blueGoldText == null && _redGoldText == null)
                {
                    UpdateCombinedGold();
                }
            }
        }

        private void UnsubscribeFromPlayerController()
        {
            if (_bluePlayerController != null)
            {
                _bluePlayerController.OnGoldChanged -= OnBlueGoldChanged;
            }
            if (_redPlayerController != null)
            {
                _redPlayerController.OnGoldChanged -= OnRedGoldChanged;
            }
        }

        private void OnBlueGoldChanged(int newGold)
        {
            UpdateBlueGold(newGold);
            UpdateCombinedGold(); // Обновляем комбинированное поле, если используется
        }

        private void OnRedGoldChanged(int newGold)
        {
            UpdateRedGold(newGold);
            UpdateCombinedGold(); // Обновляем комбинированное поле, если используется
        }

        private void UpdateBlueGold(int gold)
        {
            if (_blueGoldText != null)
            {
                _blueGoldText.text = $"Blue: {gold}";
            }
        }

        private void UpdateRedGold(int gold)
        {
            if (_redGoldText != null)
            {
                _redGoldText.text = $"Red: {gold}";
            }
        }

        private void UpdateCombinedGold()
        {
            // Если используется одно поле _goldText, показываем оба значения
            if (_goldText != null)
            {
                int blueGold = _bluePlayerController != null ? _bluePlayerController.Gold : 0;
                int redGold = _redPlayerController != null ? _redPlayerController.Gold : 0;
                _goldText.text = $"Blue: {blueGold} | Red: {redGold}";
            }
        }

        private void UpdatePlayerColor(Player playerColor)
        {
            if (_playerColorText != null)
            {
                string colorName = playerColor == Player.Blue ? "Blue" : "Red";
                _playerColorText.text = $"Player: {colorName}";
            }
        }

        private void InitializeNetworkTracking()
        {
            _networkGameManager = NetworkGameManager.Instance;
            _connectionManager = Network.NetworkConnectionManager.Instance;

            if (_networkGameManager != null)
            {
                SubscribeToNetworkEvents();
                UpdateConnectionStatus();
                
                // Показываем текущего игрока
                ShowCurrentPlayer();
            }
            else
            {
                Debug.LogWarning("NetworkGameManager.Instance is null in GameplayHUD");
            }
        }

        private void SubscribeToNetworkEvents()
        {
            if (_networkGameManager != null)
            {
                _networkGameManager.OnPlayerConnected += OnPlayerConnected;
                _networkGameManager.OnPlayerDisconnected += OnPlayerDisconnected;
                _networkGameManager.OnServerStarted += OnServerStarted;
                _networkGameManager.OnClientConnected += OnClientConnected;
            }
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (_networkGameManager != null)
            {
                _networkGameManager.OnPlayerConnected -= OnPlayerConnected;
                _networkGameManager.OnPlayerDisconnected -= OnPlayerDisconnected;
                _networkGameManager.OnServerStarted -= OnServerStarted;
                _networkGameManager.OnClientConnected -= OnClientConnected;
            }
        }

        private void OnServerStarted()
        {
            UpdateConnectionStatus();
            Debug.Log("Server started - showing host status");
        }

        private void OnClientConnected()
        {
            UpdateConnectionStatus();
            Debug.Log("Client connected - showing client status");
        }

        private void OnPlayerConnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} connected - adding to list");
            AddPlayerToList(clientId);
        }

        private void OnPlayerDisconnected(ulong clientId)
        {
            Debug.Log($"Player {clientId} disconnected - removing from list");
            RemovePlayerFromList(clientId);
        }

        private void ShowCurrentPlayer()
        {
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsClient)
            {
                ulong localClientId = networkManager.LocalClientId;
                AddPlayerToList(localClientId);
            }
        }

        private void AddPlayerToList(ulong clientId)
        {
            if (_playerEntries.ContainsKey(clientId))
            {
                return; // Уже добавлен
            }

            // Получаем имя игрока
            string playerName = GetPlayerName(clientId);
            bool isHost = IsHost(clientId);

            // Если UI элементы настроены, создаем визуальный элемент
            if (_playerEntryPrefab != null && _playersListContainer != null)
            {
                GameObject entryObj = Instantiate(_playerEntryPrefab, _playersListContainer);
                _playerEntries[clientId] = entryObj;

                // Обновляем текст
                TextMeshProUGUI nameText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    string status = isHost ? " [Host]" : " [Client]";
                    nameText.text = $"{playerName} (ID: {clientId}){status}";
                }
            }
            else
            {
                // Если UI не настроен, просто добавляем в словарь для OnGUI
                _playerEntries[clientId] = null; // null означает, что элемент только в словаре для OnGUI
                Debug.Log($"[GameplayHUD] Player {clientId} ({playerName}) added to list (UI not configured, using OnGUI)");
                return;
            }

            Debug.Log($"[GameplayHUD] Added player {clientId} ({playerName}) to list");
        }

        private void RemovePlayerFromList(ulong clientId)
        {
            if (_playerEntries.TryGetValue(clientId, out GameObject entry))
            {
                // Удаляем визуальный элемент только если он был создан
                if (entry != null)
                {
                    Destroy(entry);
                }
                _playerEntries.Remove(clientId);
                Debug.Log($"[GameplayHUD] Removed player {clientId} from list");
            }
        }

        private string GetPlayerName(ulong clientId)
        {
            // Пытаемся получить имя из Lobby
            if (_connectionManager?.JoinedLobby != null)
            {
                try
                {
                    // Для упрощения используем имя из Lobby по индексу
                    // В реальной реализации нужно синхронизировать clientId с playerId из Lobby
                    var players = _connectionManager.JoinedLobby.Players;
                    if (players != null && players.Count > 0)
                    {
                        // Простое сопоставление: используем порядковый номер
                        int index = (int)(clientId % (ulong)players.Count);
                        if (index < players.Count && players[index].Data.ContainsKey("PlayerName"))
                        {
                            return players[index].Data["PlayerName"].Value;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to get player name from lobby: {e.Message}");
                }
            }

            return $"Player {clientId}";
        }

        private bool IsHost(ulong clientId)
        {
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsServer)
            {
                // Хост всегда имеет clientId = 0
                return clientId == 0;
            }
            return false;
        }

        private void UpdateConnectionStatus()
        {
            string statusText = GetStatusText();

            // Обновляем UI элемент если он настроен
            if (_connectionStatusText != null)
            {
                _connectionStatusText.text = statusText;
            }

            // Логируем статус для отладки
            Debug.Log($"[GameplayHUD] Connection Status: {statusText}");
        }

        private string GetStatusText()
        {
            if (_networkGameManager == null)
            {
                return "Status: Not Connected";
            }

            if (_networkGameManager.IsHost)
            {
                return "Status: Host";
            }
            else if (_networkGameManager.IsClient)
            {
                return "Status: Client";
            }
            else
            {
                return "Status: Disconnected";
            }
        }

        /// <summary>
        /// Простая визуализация через OnGUI для быстрого тестирования без настройки UI
        /// </summary>
        private void OnGUI()
        {
            // Показываем статус подключения в левом верхнем углу
            string statusText = GetStatusText();
            GUI.Label(new Rect(10, 10, 300, 20), statusText);

            // Показываем золото обоих игроков и цвет локального игрока
            int blueGold = _bluePlayerController != null ? _bluePlayerController.Gold : 0;
            int redGold = _redPlayerController != null ? _redPlayerController.Gold : 0;
            GUI.Label(new Rect(10, 35, 300, 20), $"Blue Gold: {blueGold}");
            GUI.Label(new Rect(10, 60, 300, 20), $"Red Gold: {redGold}");
            
            if (_localPlayerController != null)
            {
                string colorText = $"You: {_localPlayerController.PlayerColor}";
                GUI.Label(new Rect(10, 85, 300, 20), colorText);
            }

            // Показываем количество подключенных игроков
            if (_networkGameManager != null)
            {
                int playerCount = _playerEntries.Count;
                int yOffset = 110;
                GUI.Label(new Rect(10, yOffset, 300, 20), $"Players Connected: {playerCount}");
                yOffset += 25;

                // Показываем список игроков
                foreach (var kvp in _playerEntries)
                {
                    ulong clientId = kvp.Key;
                    string playerName = GetPlayerName(clientId);
                    bool isHost = IsHost(clientId);
                    string role = isHost ? "[Host]" : "[Client]";
                    GUI.Label(new Rect(10, yOffset, 400, 20), $"- {playerName} (ID: {clientId}) {role}");
                    yOffset += 20;
                }
            }
        }

        public void UpdateGold(int newAmount)
        {
            // Устаревший метод для обратной совместимости
            // Используется только если нет отдельных полей для Blue/Red
            if (_goldText != null && _blueGoldText == null && _redGoldText == null)
            {
                _goldText.text = $"Gold: {newAmount}";
            }
        }
    }
}
