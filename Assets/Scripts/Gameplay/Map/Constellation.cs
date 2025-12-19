using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class Constellation : MonoBehaviour
    {
        [Header("Constellation Data")]
        [SerializeField] private List<Star> _stars = new List<Star>();
        [SerializeField] private Vector2 _center;
        [SerializeField] private int _owner = -1; // -1 for no owner
        [SerializeField] private int _constellationId;

        private bool _hasOwner => _owner != -1;

        // Getters
        public Vector2 Center => _center;
        public int Owner => _owner;
        public bool HasOwner => _hasOwner;
        public int ConstellationId => _constellationId;
        public List<Star> Stars => new List<Star>(_stars); // Return copy for safety

        // Setters
        public void SetCenter(Vector2 center)
        {
            _center = center;
        }

        public void SetOwner(int ownerId)
        {
            _owner = ownerId;
        }

        public void Initialize(int id, Vector2 center)
        {
            _constellationId = id;
            _center = center;
            _owner = -1;
            _stars.Clear();
        }

        public void AddStar(Star star)
        {
            if (!_stars.Contains(star))
            {
                _stars.Add(star);
            }
        }

        public int Size()
        {
            return _stars.Count;
        }

        public List<Star> GetStars()
        {
            return new List<Star>(_stars);
        }

        public bool IsFullyOwnedBy(int playerId)
        {
            if (_stars.Count == 0) return false;

            foreach (var star in _stars)
            {
                if (star.Owner != playerId)
                    return false;
            }
            return true;
        }

        public void UpdateOwnership()
        {
            // Check if all stars belong to the same player
            if (_stars.Count == 0) return;

            int firstOwner = _stars[0].Owner;
            if (firstOwner == -1) return;

            bool allSameOwner = true;
            foreach (var star in _stars)
            {
                if (star.Owner != firstOwner)
                {
                    allSameOwner = false;
                    break;
                }
            }

            if (allSameOwner)
            {
                SetOwner(firstOwner);
            }
            else
            {
                SetOwner(-1);
            }
        }
    }
}
