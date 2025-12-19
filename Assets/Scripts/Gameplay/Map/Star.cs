using UnityEngine;

namespace Gameplay.Map
{
    public class Star : MonoBehaviour
    {
        [Header("Star Data")]
        [SerializeField] private Vector2 _coordinates;
        [SerializeField] private int _owner = -1; // -1 for no owner
        [SerializeField] private int _size; // 1-5
        [SerializeField] private float _value;

        private bool _hasOwner => _owner != -1;

        // Getters
        public Vector2 Coordinates => _coordinates;
        public int Owner => _owner;
        public bool HasOwner => _hasOwner;
        public int Size => _size;
        public float Value => _value;

        // Setters
        public void SetCoordinates(Vector2 coordinates)
        {
            _coordinates = coordinates;
            transform.position = new Vector3(coordinates.x, coordinates.y, 0);
        }

        public void SetOwner(int ownerId)
        {
            _owner = ownerId;
            UpdateVisuals();
        }

        public void SetSize(int size)
        {
            _size = Mathf.Clamp(size, 1, 5);
            UpdateScale();
        }

        public void SetValue(float value)
        {
            _value = value;
        }

        public void Initialize(Vector2 coordinates, int size, float initialValue)
        {
            SetCoordinates(coordinates);
            SetSize(size);
            SetValue(initialValue);
            _owner = -1;
        }

        public void ChangeValue(float amount)
        {
            _value += amount;
            if (_value <= 0 && _hasOwner)
            {
                // Star loses ownership when value drops to 0 or below
                SetOwner(-1);
            }
        }

        private void UpdateVisuals()
        {
            // TODO: Update visual color based on owner
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
