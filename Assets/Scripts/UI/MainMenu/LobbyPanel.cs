using Network;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LobbyPanel : UIPanel
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text lobbyTitle;
        [SerializeField] private TMP_Text hostIPText; // Для отображения IP хоста (опционально)

        [SerializeField] private Transform entriesContainer;
        [SerializeField] private PlayerUIEntry playerEntryPrefab;
        private List<PlayerUIEntry> playerEntries;
        NetworkConnectionManager connectionManager;
        override protected void Start()
        {
            base.Start();

            startButton.onClick.AddListener(OnStartClicked);
            backButton.onClick.AddListener(OnBackClicked);
            //SpawnEntries();
        }

        private void OnEnable()
        {
            connectionManager = NetworkConnectionManager.Instance;
            connectionManager.OnJoinedLobby += UpdateLobby;
            connectionManager.OnJoinedLobbyUpdate += UpdateLobby;
        }

        private void OnDisable()
        {
            connectionManager.OnJoinedLobby -= UpdateLobby;
            connectionManager.OnJoinedLobbyUpdate -= UpdateLobby;
        }

        private void UpdateLobby(Lobby lobby)
        {
            if (lobby == null) return;
            
            lobbyTitle.text = lobby.Name + "  " + lobby.Players.Count + "/" + lobby.MaxPlayers;
            SpawnEntries(lobby.Players);
            
            // Отображаем IP хоста для информации
            if (hostIPText != null && connectionManager != null)
            {
                string hostIP = connectionManager.GetHostIPFromLobby();
                int hostPort = connectionManager.GetHostPortFromLobby();
                if (!string.IsNullOrEmpty(hostIP))
                {
                    hostIPText.text = $"Host IP: {hostIP}:{hostPort}";
                }
                else if (connectionManager.IsHost())
                {
                    // Если мы хост, показываем наш IP
                    string localIP = NetworkIPHelper.GetLocalIPAddress();
                    int port = NetworkIPHelper.GetDefaultPort();
                    hostIPText.text = $"Your IP: {localIP}:{port}";
                }
                else
                {
                    hostIPText.text = "Waiting for host...";
                }
            }
            
            // Кнопка Start активна только для хоста и если есть хотя бы 1 игрок (для 2 игроков нужно 2)
            if (connectionManager != null)
            {
                bool canStart = connectionManager.IsHost() && 
                               playerEntries != null && 
                               playerEntries.Count >= 1; // Можно начать с 1 игроком, но лучше дождаться 2
                startButton.interactable = canStart;
                
                // Показываем подсказку, если не хватает игроков
                if (connectionManager.IsHost() && playerEntries != null && playerEntries.Count < 2)
                {
                    Debug.Log($"[LobbyPanel] Waiting for players: {playerEntries.Count}/2");
                }
            }
            else
            {
                startButton.interactable = false;
            }

            // Автоматическое подключение клиента при обнаружении GameStarted
            if (!connectionManager.IsHost() && connectionManager.IsGameStarted())
            {
                string hostIP = connectionManager.GetHostIPFromLobby();
                int hostPort = connectionManager.GetHostPortFromLobby();

                if (!string.IsNullOrEmpty(hostIP))
                {
                    Debug.Log($"Game started detected. Connecting as client to {hostIP}:{hostPort}");
                    // Загружаем сцену Game, там NetworkGameManager подключится
                    SceneManager.LoadSceneAsync("Game");
                }
            }
        }

        private void SpawnEntries(List<Player> players)
        {
            if (playerEntries != null && players.Count != 0)
            {
                foreach (var entry in playerEntries)
                {
                    Destroy(entry.gameObject);
                }
            }
            else if (players.Count == 0)
            {
                return;
            }
            Debug.Log(players);
            playerEntries = new List<PlayerUIEntry>();

            foreach (Player player in players)
            {
                PlayerUIEntry entry = Instantiate(playerEntryPrefab, entriesContainer);
                playerEntries.Add(entry);
                entry.Initialize(player.Data["PlayerName"].Value, playerEntries.Count - 1);
            }
        }

        public async void OnStartClicked()
        {
            if (connectionManager == null)
            {
                Debug.LogError("[LobbyPanel] NetworkConnectionManager.Instance is null!");
                return;
            }

            // Отключаем кнопку на время обработки
            startButton.interactable = false;

            try
            {
                if (connectionManager.IsHost())
                {
                    Debug.Log("[LobbyPanel] Host starting game...");
                    
                    // Хост: получаем IP и порт автоматически
                    string ip = NetworkIPHelper.GetLocalIPAddress();
                    int port = NetworkIPHelper.GetDefaultPort();
                    
                    Debug.Log($"[LobbyPanel] Host IP: {ip}, Port: {port}");

                    // Обновляем Lobby Data с IP:Port
                    await connectionManager.UpdateLobbyHostIP(ip, port);

                    // Устанавливаем флаг GameStarted
                    await connectionManager.UpdateLobbyGameStarted(true);

                    Debug.Log("[LobbyPanel] Loading Game scene...");
                    // Загружаем сцену Game, там GameSceneInitializer запустит Host
                    SceneManager.LoadSceneAsync("Game");
                }
                else
                {
                    Debug.Log("[LobbyPanel] Client loading Game scene...");
                    // Клиент: просто загружаем сцену (подключение произойдет автоматически через UpdateLobby)
                    SceneManager.LoadSceneAsync("Game");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LobbyPanel] Error starting game: {e.Message}");
                startButton.interactable = true; // Включаем кнопку обратно при ошибке
            }
        }

        public void OnBackClicked()
        {
            Fader.CanvasGroup.interactable = false;
            NetworkConnectionManager.Instance.LeaveLobby();
            panelsManager.ActivatePanel(MainMenuPanels.HostGame);
        }

    }
}
