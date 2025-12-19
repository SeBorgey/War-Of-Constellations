using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Gameplay.Map
{
    public class GameMap : MonoBehaviour
    {
        private const int MaxNeighborsPerConstellation = 5;

        [Header("Map Data")]
        [SerializeField] private List<Constellation> _constellations = new List<Constellation>();

        public List<Constellation> GetConstellations()
        {
            return new List<Constellation>(_constellations);
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
                if (constellation.Id == id)
                {
                    return constellation;
                }
            }
            return null;
        }

        public Star GetStarById(int id)
        {
            foreach (var constellation in _constellations)
            {
                foreach (var star in constellation.GetStars())
                {
                    if (star.Id == id)
                    {
                        return star;
                    }
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
                if (constellation.Id == excludeConstellationId)
                    continue;

                float distance = Vector2.Distance(centerPosition, constellation.Center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }

        /// <summary>
        /// Computes constellation neighbors using Delaunay triangulation.
        /// Each constellation is limited to MaxNeighborsPerConstellation neighbors (closest ones kept).
        /// Call this after all constellations have been added.
        /// </summary>
        public void ComputeConstellationNeighbors()
        {
            if (_constellations.Count < 2) return;

            var centers = _constellations.Select(c => c.Center).ToList();
            var edges = DelaunayTriangulation.Triangulate(centers);

            // Build a dictionary of potential neighbors with distances
            var neighborCandidates = new Dictionary<Constellation, List<(Constellation neighbor, float distance)>>();
            foreach (var c in _constellations)
            {
                neighborCandidates[c] = new List<(Constellation, float)>();
            }

            foreach (var edge in edges)
            {
                var a = FindConstellationByCenter(edge.A);
                var b = FindConstellationByCenter(edge.B);

                if (a == null || b == null || a == b) continue;

                float distance = Vector2.Distance(edge.A, edge.B);
                neighborCandidates[a].Add((b, distance));
                neighborCandidates[b].Add((a, distance));
            }

            // For each constellation, keep only the closest MaxNeighborsPerConstellation neighbors
            foreach (var constellation in _constellations)
            {
                var sorted = neighborCandidates[constellation]
                    .OrderBy(x => x.distance)
                    .Take(MaxNeighborsPerConstellation)
                    .ToList();

                foreach (var (neighbor, _) in sorted)
                {
                    constellation.AddNeighborId(neighbor.Id);
                }
            }
        }

        private Constellation FindConstellationByCenter(Vector2 center)
        {
            foreach (var constellation in _constellations)
            {
                if (Vector2.Distance(constellation.Center, center) < 0.001f)
                {
                    return constellation;
                }
            }
            return null;
        }
    }
}
