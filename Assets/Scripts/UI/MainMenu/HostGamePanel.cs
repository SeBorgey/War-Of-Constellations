using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HostGamePanel : UIPanel
    {
        [SerializeField] private TMP_InputField lobbyNameField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button backButton;

        override protected void Start()
        {
            base.Start();

            hostButton.onClick.AddListener(OnHostClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        public async void OnHostClicked()
        {
            Fader.CanvasGroup.interactable = false;
            // Хардкод: только 2 игрока
            await NetworkConnectionManager.Instance.CreateLobbyAndHost(lobbyNameField.text, 2);
            panelsManager.ActivatePanel(MainMenuPanels.Lobby);
        }

        public void OnBackClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.StartGame);
        }

    }
}
