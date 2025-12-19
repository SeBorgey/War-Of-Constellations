using UnityEngine;
using System;
using Gameplay.Map;

namespace Gameplay.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Data")]
        [SerializeField] private Player _playerColor;
        [SerializeField] private int _gold;
        [SerializeField] private int _clickPower = 1;

        public Player PlayerColor => _playerColor;
        public int Gold => _gold;

        public event Action<int> OnGoldChanged;

        public void Initialize(Player color)
        {
            _playerColor = color;
            _gold = 0;
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

        public int GetClickPower()
        {
            // TODO: Apply modifiers
            return _clickPower;
        }
    }
}
