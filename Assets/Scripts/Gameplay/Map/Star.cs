using UnityEngine;

namespace Gameplay.Map
{
    public enum StarState
    {
        White,  // Neutral
        Blue,   // Controlled by Blue player
        Red     // Controlled by Red player
    }

    public enum Player
    {
        Blue,
        Red
    }

    public class Star : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private int _id;
        [SerializeField] private int _constellationId = -1; // -1 if not part of a constellation

        [Header("Star Data")]
        [SerializeField] private Vector2 _coordinates;
        [SerializeField] private int _size; // 1-5
        [SerializeField] private int _hp; // 1-20

        [Header("Combat State")]
        [SerializeField] private int _blueDamage;
        [SerializeField] private int _redDamage;

        // Getters
        public int Id => _id;
        public int ConstellationId => _constellationId;
        public Vector2 Coordinates => _coordinates;
        public int Size => _size;
        public int HP => _hp;
        public int BlueDamage => _blueDamage;
        public int RedDamage => _redDamage;

        public StarState State
        {
            get
            {
                if (_hp <= 0) return StarState.White;
                if (_blueDamage >= _hp)
                    return StarState.Blue;
                if (_redDamage >= _hp)
                    return StarState.Red;
                return StarState.White;
            }
        }

        // Setters
        public void SetId(int id)
        {
            _id = id;
        }

        public void SetConstellationId(int constellationId)
        {
            _constellationId = constellationId;
        }

        public void SetCoordinates(Vector2 coordinates)
        {
            _coordinates = coordinates;
            transform.position = new Vector3(coordinates.x, coordinates.y, 0);
        }

        public void SetSize(int size)
        {
            _size = Mathf.Clamp(size, 1, 5);
            UpdateScale();
        }

        public void SetHP(int hp)
        {
            _hp = Mathf.Clamp(hp, 1, 20);
        }

        public void Initialize(int id, Vector2 coordinates, int size, int hp)
        {
            SetId(id);
            SetCoordinates(coordinates);
            SetSize(size);
            SetHP(hp);
            _blueDamage = 0;
            _redDamage = 0;
            _constellationId = -1;
        }

        /// <summary>
        /// Apply damage to the star from a specific player.
        /// If the enemy has accumulated damage, it decreases their counter instead.
        /// </summary>
        public void ApplyDamage(Player player, int amount)
        {
            if (amount <= 0) return;

            if (player == Player.Blue)
            {
                // If Red has damage, decrease it first
                if (_redDamage > 0)
                {
                    _redDamage = Mathf.Max(0, _redDamage - amount);
                }
                else
                {
                    _blueDamage += amount;
                }
            }
            else // Player.Red
            {
                // If Blue has damage, decrease it first
                if (_blueDamage > 0)
                {
                    _blueDamage = Mathf.Max(0, _blueDamage - amount);
                }
                else
                {
                    _redDamage += amount;
                }
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // TODO: Update visual color based on State
            // This will be implemented when we add visual feedback
        }

        private void UpdateScale()
        {
            // Scale the star based on its size (1-5)
            float scale = 0.5f + (_size * 0.2f); // Size 1 = 0.7, Size 5 = 1.5
            transform.localScale = Vector3.one * scale;
        }
    }
}
