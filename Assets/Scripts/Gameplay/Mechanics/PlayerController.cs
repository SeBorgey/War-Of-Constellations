using UnityEngine;
using System;

namespace Gameplay.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Data")]
        [SerializeField] private int _playerId;
        [SerializeField] private int _gold;
        [SerializeField] private float _clickPower = 1.0f;

        public int PlayerId => _playerId;
        public int Gold => _gold;

        public event Action<int> OnGoldChanged;

        public void Initialize(int id)
        {
            _playerId = id;
            _gold = 0;
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            OnGoldChanged?.Invoke(_gold);
        }

        public float GetClickPower()
        {
            // TODO: Apply modifiers
            return _clickPower;
        }
    }
}
