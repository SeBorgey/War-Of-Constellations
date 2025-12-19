using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Gameplay.Mechanics;
using Gameplay.Map;

namespace UI
{
    /// <summary>
    /// Панель для отображения результата игры (победа/поражение)
    /// </summary>
    public class GameEndPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _panelRoot; // Root GameObject панели (для включения/выключения)
        [SerializeField] private TextMeshProUGUI _resultText; // Текст результата ("Победа!" / "Поражение!")
        [SerializeField] private TextMeshProUGUI _winnerText; // Текст с информацией о победителе
        [SerializeField] private Button _mainMenuButton; // Кнопка возврата в главное меню

        [Header("References")]
        [SerializeField] private GameWinCondition _gameWinCondition;

        private void Start()
        {
            // Скрываем панель при старте
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }

            // Находим GameWinCondition если не установлен
            if (_gameWinCondition == null)
            {
                _gameWinCondition = UnityEngine.Object.FindFirstObjectByType<GameWinCondition>();
            }

            // Подписываемся на события
            if (_gameWinCondition != null)
            {
                _gameWinCondition.OnPlayerWon += OnPlayerWon;
                _gameWinCondition.OnPlayerLost += OnPlayerLost;
                Debug.Log("[GameEndPanel] Subscribed to GameWinCondition events");
            }
            else
            {
                Debug.LogWarning("[GameEndPanel] GameWinCondition not found!");
            }

            // Настраиваем кнопку
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(LoadMainMenu);
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_gameWinCondition != null)
            {
                _gameWinCondition.OnPlayerWon -= OnPlayerWon;
                _gameWinCondition.OnPlayerLost -= OnPlayerLost;
            }
        }

        private void OnPlayerWon(Player winner)
        {
            ShowResult(true, winner);
        }

        private void OnPlayerLost(Player loser)
        {
            ShowResult(false, loser);
        }

        private void ShowResult(bool isVictory, Player player)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            // Обновляем текст результата
            if (_resultText != null)
            {
                if (isVictory)
                {
                    _resultText.text = "ПОБЕДА!";
                    _resultText.color = Color.green;
                }
                else
                {
                    _resultText.text = "ПОРАЖЕНИЕ";
                    _resultText.color = Color.red;
                }
            }

            // Обновляем текст о победителе
            if (_winnerText != null)
            {
                string playerName = player == Player.Blue ? "Blue" : "Red";
                if (isVictory)
                {
                    _winnerText.text = $"Вы победили! ({playerName})";
                }
                else
                {
                    string winnerName = player == Player.Blue ? "Red" : "Blue";
                    _winnerText.text = $"Победитель: {winnerName}";
                }
            }

            Debug.Log($"[GameEndPanel] Showing result: {(isVictory ? "Victory" : "Defeat")} for {player}");
        }

        private void LoadMainMenu()
        {
            Debug.Log("[GameEndPanel] Loading MainMenu scene...");
            
            // Отключаем сеть перед загрузкой сцены
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null)
            {
                if (networkManager.IsServer)
                {
                    networkManager.Shutdown();
                }
                else if (networkManager.IsClient)
                {
                    networkManager.Shutdown();
                }
            }

            // Загружаем главное меню
            SceneManager.LoadScene("MainMenu");
        }
    }
}

