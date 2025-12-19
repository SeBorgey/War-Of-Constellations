using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class GameMap : MonoBehaviour
    {
        [Header("Map Data")]
        [SerializeField] private List<Constellation> _constellations = new List<Constellation>();

        public List<Constellation> GetConstellations()
        {
            return [.. _constellations];
        }

        public void AddConstellation(Constellation constellation)
        {
            if (!_constellations.Contains(constellation))
            {
                _constellations.Add(constellation);
            }
        }

        public int Size()
        {
            return _constellations.Count;
        }

        public void Clear()
        {
            // Destroy all constellation GameObjects
            foreach (var constellation in _constellations)
            {
                if (constellation != null)
                {
                    Destroy(constellation.gameObject);
                }
            }
            _constellations.Clear();
        }

        public Constellation GetConstellationById(int id)
        {
            foreach (var constellation in _constellations)
            {
                if (constellation.ConstellationId == id)
                {
                    return constellation;
                }
            }
            return null;
        }

        public Star FindClosestStar(Vector2 position)
        {
            Star closest = null;
            float minDistance = float.MaxValue;

            foreach (var constellation in _constellations)
            {
                foreach (var star in constellation.GetStars())
                {
                    float distance = Vector2.Distance(position, star.Coordinates);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = star;
                    }
                }
            }

            return closest;
        }

        public float GetDistanceToClosestConstellation(Vector2 centerPosition, int excludeConstellationId = -1)
        {
            float minDistance = float.MaxValue;

            foreach (var constellation in _constellations)
            {
                if (constellation.ConstellationId == excludeConstellationId)
                    continue;

                float distance = Vector2.Distance(centerPosition, constellation.Center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }
    }
}
