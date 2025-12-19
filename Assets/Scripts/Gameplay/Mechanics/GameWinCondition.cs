using Unity.Netcode;
using UnityEngine;
using System;
using Gameplay.Map;

namespace Gameplay.Mechanics
{
    /// <summary>
    /// Проверяет условия победы/поражения в игре
    /// </summary>
    public class GameWinCondition : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _checkInterval = 2f; // Проверка каждые 2 секунды
        [SerializeField] private GameMap _gameMap;

        public event Action<Player> OnPlayerWon;
        public event Action<Player> OnPlayerLost;

        private float _lastCheckTime;
        private bool _gameEnded = false;

        private void Start()
        {
            if (_gameMap == null)
            {
                _gameMap = UnityEngine.Object.FindFirstObjectByType<GameMap>();
            }
        }

        private void Update()
        {
            // Проверка условий победы должна происходить только на сервере
            if (!IsServer || _gameEnded)
            {
                return;
            }

            if (Time.time - _lastCheckTime >= _checkInterval)
            {
                _lastCheckTime = Time.time;
                CheckWinConditions();
            }
        }

        private void CheckWinConditions()
        {
            if (_gameMap == null)
            {
                return;
            }

            int blueStars = 0;
            int redStars = 0;
            int neutralStars = 0;

            // Подсчитываем узлы каждого игрока
            var constellations = _gameMap.GetConstellations();
            foreach (var constellation in constellations)
            {
                var stars = constellation.GetStars();
                foreach (var star in stars)
                {
                    switch (star.State)
                    {
                        case StarState.Blue:
                            blueStars++;
                            break;
                        case StarState.Red:
                            redStars++;
                            break;
                        case StarState.White:
                        default:
                            neutralStars++;
                            break;
                    }
                }
            }

            int totalStars = blueStars + redStars + neutralStars;
            
            // Если все узлы захвачены одним игроком
            if (neutralStars == 0 && totalStars > 0)
            {
                if (blueStars > 0 && redStars == 0)
                {
                    // Blue победил
                    EndGame(Player.Blue);
                    return;
                }
                else if (redStars > 0 && blueStars == 0)
                {
                    // Red победил
                    EndGame(Player.Red);
                    return;
                }
            }

            // Проверяем, есть ли у игроков хотя бы один узел
            // Если у одного игрока нет узлов, он проиграл
            if (blueStars == 0 && redStars > 0)
            {
                // Blue проиграл
                EndGame(Player.Red);
                return;
            }
            else if (redStars == 0 && blueStars > 0)
            {
                // Red проиграл
                EndGame(Player.Blue);
                return;
            }

            // Логируем текущее состояние (для отладки)
            Debug.Log($"[GameWinCondition] Blue: {blueStars}, Red: {redStars}, Neutral: {neutralStars}");
        }

        private void EndGame(Player winner)
        {
            if (_gameEnded)
            {
                return;
            }

            _gameEnded = true;
            Debug.Log($"[GameWinCondition] Game ended! Winner: {winner}");

            // Уведомляем всех клиентов о победе/поражении
            NotifyGameEndClientRpc((int)winner);
        }

        [ClientRpc]
        private void NotifyGameEndClientRpc(int winnerInt)
        {
            Player winner = (Player)winnerInt;
            
            // Определяем локального игрока
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            Player localPlayer = networkManager != null && networkManager.IsHost 
                ? Player.Blue 
                : Player.Red;

            if (winner == localPlayer)
            {
                OnPlayerWon?.Invoke(winner);
                Debug.Log($"[GameWinCondition] You won!");
            }
            else
            {
                OnPlayerLost?.Invoke(localPlayer);
                Debug.Log($"[GameWinCondition] You lost! Winner: {winner}");
            }
        }

        public int GetPlayerStarCount(Player player)
        {
            if (_gameMap == null)
            {
                return 0;
            }

            int count = 0;
            StarState targetState = player == Player.Blue ? StarState.Blue : StarState.Red;

            var constellations = _gameMap.GetConstellations();
            foreach (var constellation in constellations)
            {
                var stars = constellation.GetStars();
                foreach (var star in stars)
                {
                    if (star.State == targetState)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}

