using UnityEngine;

namespace UI
{
    public enum MainMenuPanels
    {
        Main,
        StartGame,
        HostGame,
        JoinGame,
        Lobby,
    }

    public class MainMenuPanelsManager : MonoBehaviour
    {
        public static MainMenuPanelsManager Instance;

        [SerializeField] private UIPanel mainPanel;
        [SerializeField] private UIPanel startGamePanel;
        [SerializeField] private UIPanel hostGamePanel;
        [SerializeField] private UIPanel joinGamePanel;
        [SerializeField] private UIPanel lobbyPannel;

        protected UIPanel activePanel;

        private void OnEnable()
        {
            Instance = this;
        }

        private void Start()
        {
            ActivatePanel(mainPanel);
        }

        public void ActivatePanel(MainMenuPanels panelToActivate)
        {
            switch (panelToActivate)
            {
                case MainMenuPanels.Main: 
                    ActivatePanel(mainPanel); 
                    break;
                case MainMenuPanels.StartGame: 
                    ActivatePanel(startGamePanel); 
                    break;
                case MainMenuPanels.HostGame:
                    ActivatePanel(hostGamePanel);
                    break;
                case MainMenuPanels.JoinGame:
                    ActivatePanel(joinGamePanel);
                    break;
                case MainMenuPanels.Lobby:
                    ActivatePanel(lobbyPannel);
                    break;
            }
        }

        private void ActivatePanel(UIPanel panel)
        {
            if (panel == null)
            {
                Debug.Log("No such panel..");
                return;
            }
            if(activePanel != null) activePanel.Fader.FadePanel(false);
      
            activePanel = panel;
            panel.Fader.FadePanel(true);
        }
    }
}