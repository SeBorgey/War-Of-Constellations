using UnityEngine;
using System;

namespace Core
{
    public enum GameState
    {
        WaitingForPlayers,
        Playing,
        Finished
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"Game State Changed to: {newState}");
        }

        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        public void EndGame(string winnerId)
        {
            SetState(GameState.Finished);
            Debug.Log($"Game Over. Winner: {winnerId}");
        }
    }
}
