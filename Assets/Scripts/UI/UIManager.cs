using UnityEngine;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _gameHUDPanel;
        [SerializeField] private GameObject _gameOverPanel;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _goldText;

        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowMainMenu()
        {
            _mainMenuPanel.SetActive(true);
            _gameHUDPanel.SetActive(false);
            _gameOverPanel.SetActive(false);
        }

        public void ShowGameHUD()
        {
            _mainMenuPanel.SetActive(false);
            _gameHUDPanel.SetActive(true);
            _gameOverPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            _gameOverPanel.SetActive(true);
        }

        public void UpdateGoldDisplay(int amount)
        {
            if (_goldText != null)
                _goldText.text = $"Gold: {amount}";
        }
    }
}
