using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuPanel : UIPanel
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button backButton;

        override protected void Start()
        {
            base.Start();
            startButton.onClick.AddListener(OnPlayClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }

        public void OnPlayClicked()
        {
            panelsManager.ActivatePanel(MainMenuPanels.StartGame);
        }

        public void OnSettingsClicked()
        {
            Debug.Log("There is no settings panel yet....");
        }

        public void OnExitClicked()
        {
            Application.Quit();
        }

    }
}
