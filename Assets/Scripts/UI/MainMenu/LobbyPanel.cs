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
            lobbyTitle.text = lobby.Name + "  " + lobby.Players.Count + "/" + lobby.MaxPlayers;
            SpawnEntries(lobby.Players);
            startButton.interactable = playerEntries.Count == 4;

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
            if (connectionManager.IsHost())
            {
                // Хост: получаем IP и порт, обновляем Lobby Data, запускаем Host
                string ip = NetworkIPHelper.GetLocalIPAddress();
                int port = NetworkIPHelper.GetDefaultPort();

                // Обновляем Lobby Data с IP:Port (на случай если они изменились)
                await connectionManager.UpdateLobbyHostIP(ip, port);

                // Устанавливаем флаг GameStarted
                await connectionManager.UpdateLobbyGameStarted(true);

                // Загружаем сцену Game, там NetworkGameManager запустит Host
                SceneManager.LoadSceneAsync("Game");
            }
            else
            {
                // Клиент: просто загружаем сцену (подключение произойдет автоматически через UpdateLobby)
                SceneManager.LoadSceneAsync("Game");
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
