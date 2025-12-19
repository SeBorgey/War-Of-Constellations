using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map.Visualization
{
    /// <summary>
    /// Visual representation of edges within a constellation.
    /// Connects all stars within the constellation using LineRenderers.
    /// </summary>
    public class ConstellationView : MonoBehaviour
    {
        private Constellation _constellation;
        private MapVisualizationSettings _settings;
        private List<LineRenderer> _edgeRenderers = new List<LineRenderer>();
        private StarState _lastOwnerState;

        public Constellation Constellation => _constellation;

        /// <summary>
        /// Initialize the ConstellationView with a Constellation reference and settings.
        /// </summary>
        public void Initialize(Constellation constellation, MapVisualizationSettings settings)
        {
            _constellation = constellation;
            _settings = settings;

            CreateEdges();
            Refresh();
        }

        private void CreateEdges()
        {
            var stars = _constellation.GetStars();
            if (stars.Count < 2) return;

            // Create edges connecting stars in a spanning tree pattern
            // Connect each star to its nearest neighbor that hasn't been connected yet
            // This creates a simple connected graph without excessive crossing lines

            HashSet<int> connected = new HashSet<int>();
            List<(int from, int to)> edges = new List<(int, int)>();

            // Start with first star
            connected.Add(0);

            while (connected.Count < stars.Count)
            {
                float minDistance = float.MaxValue;
                int bestFrom = -1;
                int bestTo = -1;

                // Find the closest unconnected star to any connected star
                foreach (int fromIdx in connected)
                {
                    for (int toIdx = 0; toIdx < stars.Count; toIdx++)
                    {
                        if (connected.Contains(toIdx)) continue;

                        float dist = Vector2.Distance(stars[fromIdx].Coordinates, stars[toIdx].Coordinates);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            bestFrom = fromIdx;
                            bestTo = toIdx;
                        }
                    }
                }

                if (bestTo >= 0)
                {
                    connected.Add(bestTo);
                    edges.Add((bestFrom, bestTo));
                }
            }

            // Also add some extra edges to create a more connected look
            // Connect stars that are close to each other
            float avgDistance = CalculateAverageDistance(stars);
            for (int i = 0; i < stars.Count; i++)
            {
                for (int j = i + 1; j < stars.Count; j++)
                {
                    float dist = Vector2.Distance(stars[i].Coordinates, stars[j].Coordinates);
                    // Add edge if distance is less than 1.5x average
                    if (dist < avgDistance * 1.5f)
                    {
                        var edge = (i, j);
                        var edgeReverse = (j, i);
                        if (!edges.Contains(edge) && !edges.Contains(edgeReverse))
                        {
                            edges.Add(edge);
                        }
                    }
                }
            }

            // Create LineRenderer for each edge
            foreach (var edge in edges)
            {
                CreateEdgeRenderer(stars[edge.from], stars[edge.to]);
            }
        }

        private float CalculateAverageDistance(List<Star> stars)
        {
            if (stars.Count < 2) return 100f;

            float totalDistance = 0f;
            int count = 0;

            for (int i = 0; i < stars.Count; i++)
            {
                for (int j = i + 1; j < stars.Count; j++)
                {
                    totalDistance += Vector2.Distance(stars[i].Coordinates, stars[j].Coordinates);
                    count++;
                }
            }

            return count > 0 ? totalDistance / count : 100f;
        }

        private void CreateEdgeRenderer(Star starA, Star starB)
        {
            var edgeObj = new GameObject($"Edge_{starA.Id}_{starB.Id}");
            edgeObj.transform.SetParent(transform);

            var lineRenderer = edgeObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(starA.Coordinates.x, starA.Coordinates.y, 0.1f));
            lineRenderer.SetPosition(1, new Vector3(starB.Coordinates.x, starB.Coordinates.y, 0.1f));

            // Setup material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = _settings.constellationEdgeWidth;
            lineRenderer.endWidth = _settings.constellationEdgeWidth;
            lineRenderer.sortingOrder = 5;

            // Use world space
            lineRenderer.useWorldSpace = true;

            _edgeRenderers.Add(lineRenderer);
        }

        private void Update()
        {
            if (_constellation == null) return;

            // Check for owner state changes
            StarState currentOwner = _constellation.GetOwner();
            if (currentOwner != _lastOwnerState)
            {
                UpdateEdgeColors();
                _lastOwnerState = currentOwner;
            }
        }

        /// <summary>
        /// Force refresh all visual elements.
        /// </summary>
        public void Refresh()
        {
            UpdateEdgeColors();
        }

        private void UpdateEdgeColors()
        {
            if (_constellation == null || _settings == null) return;

            StarState owner = _constellation.GetOwner();
            Color edgeColor = _settings.GetConstellationEdgeColor(owner);

            foreach (var renderer in _edgeRenderers)
            {
                if (renderer != null)
                {
                    renderer.startColor = edgeColor;
                    renderer.endColor = edgeColor;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var renderer in _edgeRenderers)
            {
                if (renderer != null && renderer.gameObject != null)
                {
                    Destroy(renderer.gameObject);
                }
            }
            _edgeRenderers.Clear();
            _constellation = null;
            _settings = null;
        }
    }
}
