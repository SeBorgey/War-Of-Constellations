using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class LobbyPanel : UIPanel
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        [SerializeField] private Transform entriesContainer;
        [SerializeField] private LobbyUIEntry lobbyUIEntryPrefab;
        private List<LobbyUIEntry> players;

        override protected void Start()
        {
            base.Start();

            startButton.onClick.AddListener(OnStartClicked);
            backButton.onClick.AddListener(OnBackClicked);

            //SpawnEntries();
        }

        private async void SpawnEntries()
        {

            //foreach (Lobby lobby in networkConnection.Lobbies)
            //{
            //    LobbyUIEntry UIlobby = Instantiate(lobbyUIEntryPrefab, entriesContainer);
            //    lobbies.Add(UIlobby);
            //    UIlobby.Initialize(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobbies.Count);
            //}
        }

        public void OnStartClicked()
        {
            //panelsManager.ActivatePanel(MainMenuPanels.JoinGame);
            //await NetworkConnectionManager.Instance.CreateLobbyAndHost();
        }

        public void OnBackClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.HostGame);
        }

    }
}
