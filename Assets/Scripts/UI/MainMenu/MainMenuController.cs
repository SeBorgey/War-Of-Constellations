using UnityEngine;
using Core;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnPlayClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }

        public void OnPlayClicked()
        {
            Debug.Log("Play Clicked");
            GameManager.Instance.StartGame();
            // In a real scenario, this would load the game scene or start network host
            Network.NetworkService.Instance.StartHost(); // Auto-host for now
        }

        public void OnSettingsClicked()
        {
            Debug.Log("There is no settings tab yet...");
        }

        public void OnExitClicked()
        {
            Debug.Log("Exit Clicked");
            Application.Quit();
        }
    }
}
