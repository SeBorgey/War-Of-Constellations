using UnityEngine;

namespace UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private UIPanelFader fader;
        public UIPanelFader Fader => fader;
        protected MainMenuPanelsManager panelsManager;

        virtual protected void Start()
        {
            panelsManager = MainMenuPanelsManager.Instance;
        }

    }
}

