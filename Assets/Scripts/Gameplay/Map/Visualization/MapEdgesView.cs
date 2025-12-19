using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map.Visualization
{
    /// <summary>
    /// Visual representation of edges between constellations.
    /// Shows the connection topology from Delaunay triangulation.
    /// </summary>
    public class MapEdgesView : MonoBehaviour
    {
        private List<Constellation> _constellations;
        private MapVisualizationSettings _settings;
        private List<LineRenderer> _edgeRenderers = new List<LineRenderer>();

        /// <summary>
        /// Initialize the MapEdgesView with constellation data and settings.
        /// </summary>
        public void Initialize(List<Constellation> constellations, MapVisualizationSettings settings)
        {
            _constellations = constellations;
            _settings = settings;

            CreateEdges();
            Refresh();
        }

        private void CreateEdges()
        {
            if (_constellations == null || _constellations.Count < 2) return;

            // Track which edges we've already created to avoid duplicates
            HashSet<string> createdEdges = new HashSet<string>();

            foreach (var constellation in _constellations)
            {
                foreach (int neighborId in constellation.NeighborIds)
                {
                    // Create unique edge key (always smaller ID first)
                    int minId = Mathf.Min(constellation.Id, neighborId);
                    int maxId = Mathf.Max(constellation.Id, neighborId);
                    string edgeKey = $"{minId}_{maxId}";

                    // Skip if already created
                    if (createdEdges.Contains(edgeKey)) continue;
                    createdEdges.Add(edgeKey);

                    // Find the neighbor constellation
                    Constellation neighbor = FindConstellationById(neighborId);
                    if (neighbor == null) continue;

                    CreateEdgeRenderer(constellation, neighbor);
                }
            }
        }

        private Constellation FindConstellationById(int id)
        {
            foreach (var constellation in _constellations)
            {
                if (constellation.Id == id)
                    return constellation;
            }
            return null;
        }

        private void CreateEdgeRenderer(Constellation a, Constellation b)
        {
            var edgeObj = new GameObject($"MapEdge_{a.Id}_{b.Id}");
            edgeObj.transform.SetParent(transform);

            var lineRenderer = edgeObj.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, new Vector3(a.Center.x, a.Center.y, 0.5f));
            lineRenderer.SetPosition(1, new Vector3(b.Center.x, b.Center.y, 0.5f));

            // Setup material
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = _settings.mapEdgeWidth;
            lineRenderer.endWidth = _settings.mapEdgeWidth;

            // Set color
            lineRenderer.startColor = _settings.mapEdgeColor;
            lineRenderer.endColor = _settings.mapEdgeColor;

            // Sorting - behind constellation edges
            lineRenderer.sortingOrder = 1;

            // Use world space
            lineRenderer.useWorldSpace = true;

            _edgeRenderers.Add(lineRenderer);
        }

        /// <summary>
        /// Force refresh all visual elements.
        /// </summary>
        public void Refresh()
        {
            if (_settings == null) return;

            foreach (var renderer in _edgeRenderers)
            {
                if (renderer != null)
                {
                    renderer.startColor = _settings.mapEdgeColor;
                    renderer.endColor = _settings.mapEdgeColor;
                    renderer.startWidth = _settings.mapEdgeWidth;
                    renderer.endWidth = _settings.mapEdgeWidth;
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
            _constellations = null;
            _settings = null;
        }
    }
}
