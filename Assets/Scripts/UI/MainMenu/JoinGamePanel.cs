using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Network;
using Unity.Services.Lobbies.Models;

namespace UI
{
    public class JoinGamePanel : UIPanel
    {
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshButton;

        [SerializeField] private GameObject noLobbiesPanel;
        [SerializeField] private Transform entriesContainer;
        [SerializeField] private LobbyUIEntry lobbyUIEntryPrefab;
        private List<LobbyUIEntry> lobbies;
        private int selectedId = -1;

        override protected void Start()
        {
            base.Start();

            joinButton.onClick.AddListener(OnJoinClicked);
            backButton.onClick.AddListener(OnBackClicked);
            refreshButton.onClick.AddListener(SpawnEntries);
        }

        private void OnEnable()
        {
            SpawnEntries();
        }

        private async void SpawnEntries()
        {
            NetworkConnectionManager networkConnection = NetworkConnectionManager.Instance;
            if (networkConnection == null)
            {
                Debug.LogError("[JoinGamePanel] NetworkConnectionManager.Instance is null!");
                return;
            }

            Debug.Log("[JoinGamePanel] Querying lobbies...");
            await networkConnection.QueryLobbies();

            // Очищаем старые записи
            if (lobbies != null && lobbies.Count != 0)
            {
                noLobbiesPanel.SetActive(false);
                foreach (var entry in lobbies)
                {
                    entry.OnSelected -= OnLobbySelected;
                    Destroy(entry.gameObject);
                }
            }

            if (networkConnection.LobbiesList == null || networkConnection.LobbiesList.Count == 0)
            {
                noLobbiesPanel.SetActive(true);
                Debug.Log("[JoinGamePanel] No lobbies found");
                return;
            }

            noLobbiesPanel.SetActive(false);
            lobbies = new List<LobbyUIEntry>();

            Debug.Log($"[JoinGamePanel] Found {networkConnection.LobbiesList.Count} lobby(ies)");

            foreach (Lobby lobby in networkConnection.LobbiesList)
            {
                LobbyUIEntry entry = Instantiate(lobbyUIEntryPrefab, entriesContainer);
                lobbies.Add(entry);
                entry.OnSelected += OnLobbySelected;
                entry.Initialize(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobbies.Count - 1);
                Debug.Log($"[JoinGamePanel] Added lobby: {lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})");
            }
        }

        private async void OnJoinClicked()
        {
            if (selectedId < 0 || selectedId >= lobbies.Count)
            {
                Debug.LogError("[JoinGamePanel] No lobby selected!");
                return;
            }

            NetworkConnectionManager networkConnection = NetworkConnectionManager.Instance;
            if (networkConnection == null || networkConnection.LobbiesList == null || selectedId >= networkConnection.LobbiesList.Count)
            {
                Debug.LogError("[JoinGamePanel] Invalid lobby selection!");
                return;
            }

            Fader.CanvasGroup.interactable = false;
            joinButton.interactable = false;

            try
            {
                Debug.Log($"[JoinGamePanel] Joining lobby: {networkConnection.LobbiesList[selectedId].Name}");
                await networkConnection.JoinLobby(networkConnection.LobbiesList[selectedId].Id);
                Debug.Log("[JoinGamePanel] Successfully joined lobby, switching to Lobby panel");
                panelsManager.ActivatePanel(MainMenuPanels.Lobby);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JoinGamePanel] Failed to join lobby: {e.Message}");
                joinButton.interactable = true;
                Fader.CanvasGroup.interactable = true;
            }
        }

        private void OnBackClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.StartGame);
        }

        private void OnLobbySelected(int id)
        {
            if (selectedId >= 0) lobbies[id].Desellect();
            selectedId = id;
            joinButton.interactable = true;
        }
    }
}

