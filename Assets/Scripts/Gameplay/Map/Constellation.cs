using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class Constellation : MonoBehaviour
    {
        [Header("Constellation Data")]
        [SerializeField] private int _id;
        [SerializeField] private List<Star> _stars = new List<Star>();
        [SerializeField] private Vector2 _center;
        [SerializeField] private List<int> _neighborIds = new List<int>();

        // Getters
        public int Id => _id;
        public Vector2 Center => _center;
        public List<Star> Stars => new List<Star>(_stars); // Return copy for safety
        public List<int> NeighborIds => new List<int>(_neighborIds);

        /// <summary>
        /// Returns the owner state of the constellation.
        /// Blue/Red if ALL stars are owned by that player, White otherwise.
        /// </summary>
        public StarState GetOwner()
        {
            if (_stars.Count == 0) return StarState.White;

            StarState firstState = _stars[0].State;

            // If first star is neutral, constellation is neutral
            if (firstState == StarState.White) return StarState.White;

            // Check if all stars have the same state
            foreach (var star in _stars)
            {
                if (star.State != firstState)
                    return StarState.White;
            }

            return firstState;
        }

        // Setters
        public void SetId(int id)
        {
            _id = id;
        }

        public void SetCenter(Vector2 center)
        {
            _center = center;
        }

        public void Initialize(int id, Vector2 center)
        {
            _id = id;
            _center = center;
            _stars.Clear();
            _neighborIds.Clear();
        }

        public void AddStar(Star star)
        {
            if (!_stars.Contains(star))
            {
                _stars.Add(star);
                star.SetConstellationId(_id);
            }
        }

        public void AddNeighborId(int neighborId)
        {
            if (neighborId != _id && !_neighborIds.Contains(neighborId))
            {
                _neighborIds.Add(neighborId);
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
    }
}
