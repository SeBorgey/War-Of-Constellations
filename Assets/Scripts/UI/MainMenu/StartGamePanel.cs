using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network;
using System;

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
            try
            {
                Fader.CanvasGroup.interactable = false;
                Debug.Log("[StartGamePanel] Initializing for host...");
                await NetworkConnectionManager.Instance.InitializeAsync(playerNameField.text);
                Debug.Log("[StartGamePanel] Initialization complete, switching to HostGame panel");
                panelsManager.ActivatePanel(MainMenuPanels.HostGame);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StartGamePanel] Failed to initialize: {e.Message}\n{e.StackTrace}");
                Fader.CanvasGroup.interactable = true; // Разблокируем UI при ошибке
            }
        }

        public async void OnJoinClicked()
        {
            try
            {
                Fader.CanvasGroup.interactable = false;
                Debug.Log("[StartGamePanel] Initializing for join...");
                await NetworkConnectionManager.Instance.InitializeAsync(playerNameField.text);
                Debug.Log("[StartGamePanel] Initialization complete, switching to JoinGame panel");
                panelsManager.ActivatePanel(MainMenuPanels.JoinGame);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StartGamePanel] Failed to initialize: {e.Message}\n{e.StackTrace}");
                Fader.CanvasGroup.interactable = true; // Разблокируем UI при ошибке
            }
        }

        public void OnBackClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.StartGame - 1);
        }

    }
}