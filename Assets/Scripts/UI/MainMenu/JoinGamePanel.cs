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
            await networkConnection.QueryLobbies();

            if (lobbies != null && lobbies.Count != 0)
            {
                noLobbiesPanel.SetActive(false);
                foreach (var entry in lobbies)
                {
                    entry.OnSelected -= OnLobbySelected;
                    Destroy(entry.gameObject);
                }
            }
            else if (networkConnection.LobbiesList.Count == 0)
            {
                noLobbiesPanel.SetActive(true);
                return;
            }

            lobbies = new List<LobbyUIEntry>();

            foreach (Lobby lobby in networkConnection.LobbiesList)
            {
                LobbyUIEntry entry = Instantiate(lobbyUIEntryPrefab, entriesContainer);
                lobbies.Add(entry);
                entry.OnSelected += OnLobbySelected;
                entry.Initialize(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobbies.Count - 1);
            }
        }

        private async void OnJoinClicked()
        {
            Fader.CanvasGroup.interactable = false;
            NetworkConnectionManager networkConnection = NetworkConnectionManager.Instance;
            await networkConnection.JoinLobby(networkConnection.LobbiesList[selectedId].Id);
            panelsManager.ActivatePanel(MainMenuPanels.Lobby);
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

