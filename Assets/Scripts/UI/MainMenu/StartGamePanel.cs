using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network;

namespace UI
{
    public class StartGamePanel : UIPanel
    {
        [SerializeField] private TMP_InputField playerNameField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;

        override protected void Start()
        {
            base.Start();

            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
            backButton.onClick.AddListener(OnBackClicked);
        }

        public async void OnHostClicked()
        {
            await NetworkConnectionManager.Instance.InitializeAsync(playerNameField.text);
            panelsManager.ActivatePanel(MainMenuPanels.HostGame);
        }

        public async void OnJoinClicked()
        {
            await NetworkConnectionManager.Instance.InitializeAsync(playerNameField.text);
            panelsManager.ActivatePanel(MainMenuPanels.JoinGame);
        }

        public void OnBackClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.StartGame -1);
        }

    }
}