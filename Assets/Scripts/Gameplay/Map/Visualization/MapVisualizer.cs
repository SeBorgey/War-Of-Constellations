using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map.Visualization
{
    /// <summary>
    /// Main controller for map visualization.
    /// Manages creation and updates of visual components for stars, constellations and edges.
    /// </summary>
    public class MapVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameMap _gameMap;

        [Header("Visual Settings")]
        [SerializeField] private MapVisualizationSettings _settings;

        [Header("Containers")]
        [SerializeField] private Transform _constellationEdgesContainer;
        [SerializeField] private Transform _mapEdgesContainer;

        // View collections
        private Dictionary<int, StarView> _starViews = new Dictionary<int, StarView>();
        private Dictionary<int, ConstellationView> _constellationViews = new Dictionary<int, ConstellationView>();
        private MapEdgesView _mapEdgesView;

        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        private void Awake()
        {
            CreateContainersIfNeeded();
        }

        private void CreateContainersIfNeeded()
        {
            if (_constellationEdgesContainer == null)
            {
                var edgesObj = new GameObject("ConstellationEdges");
                edgesObj.transform.SetParent(transform);
                _constellationEdgesContainer = edgesObj.transform;
            }

            if (_mapEdgesContainer == null)
            {
                var mapEdgesObj = new GameObject("MapEdges");
                mapEdgesObj.transform.SetParent(transform);
                _mapEdgesContainer = mapEdgesObj.transform;
            }
        }

        /// <summary>
        /// Initialize visualization for the current game map.
        /// Call this after map generation is complete.
        /// </summary>
        public void Initialize()
        {
            if (_gameMap == null)
            {
                Debug.LogError("[MapVisualizer] GameMap reference is not set!");
                return;
            }

            if (_settings == null)
            {
                Debug.LogWarning("[MapVisualizer] Settings not assigned, using defaults.");
                _settings = ScriptableObject.CreateInstance<MapVisualizationSettings>();
            }

            ClearVisualization();

            var constellations = _gameMap.GetConstellations();

            // Add StarView components to existing Star objects
            foreach (var constellation in constellations)
            {
                CreateConstellationView(constellation);

                foreach (var star in constellation.GetStars())
                {
                    AttachStarView(star);
                }
            }

            // Create inter-constellation edges
            CreateMapEdgesView(constellations);

            _isInitialized = true;
            Debug.Log($"[MapVisualizer] Initialized with {_starViews.Count} stars and {_constellationViews.Count} constellations");
        }

        /// <summary>
        /// Attach StarView component to an existing Star GameObject.
        /// </summary>
        private void AttachStarView(Star star)
        {
            if (star == null) return;

            // Check if StarView already exists on this Star
            var existingView = star.GetComponent<StarView>();
            if (existingView != null)
            {
                existingView.Initialize(_settings);
                _starViews[star.Id] = existingView;
                return;
            }

            // Add new StarView component to the Star's GameObject
            var starView = star.gameObject.AddComponent<StarView>();
            starView.Initialize(_settings);

            _starViews[star.Id] = starView;
        }

        private void CreateConstellationView(Constellation constellation)
        {
            var viewObj = new GameObject($"ConstellationView_{constellation.Id}");
            viewObj.transform.SetParent(_constellationEdgesContainer);

            var constellationView = viewObj.AddComponent<ConstellationView>();
            constellationView.Initialize(constellation, _settings);

            _constellationViews[constellation.Id] = constellationView;
        }

        private void CreateMapEdgesView(List<Constellation> constellations)
        {
            var viewObj = new GameObject("MapEdgesView");
            viewObj.transform.SetParent(_mapEdgesContainer);

            _mapEdgesView = viewObj.AddComponent<MapEdgesView>();
            _mapEdgesView.Initialize(constellations, _settings);
        }

        /// <summary>
        /// Clear all visualization objects.
        /// Note: StarView components on Star objects are not destroyed here
        /// because Stars are NetworkObjects managed by MapGenerator.
        /// </summary>
        public void ClearVisualization()
        {
            // Remove StarView components from Stars (they are on the same GameObject)
            foreach (var starView in _starViews.Values)
            {
                if (starView != null)
                {
                    Destroy(starView);
                }
            }
            _starViews.Clear();

            // Destroy ConstellationView GameObjects
            foreach (var constellationView in _constellationViews.Values)
            {
                if (constellationView != null)
                    Destroy(constellationView.gameObject);
            }
            _constellationViews.Clear();

            // Destroy MapEdgesView GameObject
            if (_mapEdgesView != null)
            {
                Destroy(_mapEdgesView.gameObject);
                _mapEdgesView = null;
            }

            _isInitialized = false;
        }

        /// <summary>
        /// Force refresh all visual elements.
        /// </summary>
        public void RefreshAll()
        {
            foreach (var starView in _starViews.Values)
            {
                if (starView != null)
                    starView.Refresh();
            }

            foreach (var constellationView in _constellationViews.Values)
            {
                if (constellationView != null)
                    constellationView.Refresh();
            }

            _mapEdgesView?.Refresh();
        }

        /// <summary>
        /// Get the StarView for a specific star ID.
        /// </summary>
        public StarView GetStarView(int starId)
        {
            _starViews.TryGetValue(starId, out var view);
            return view;
        }

        /// <summary>
        /// Get the ConstellationView for a specific constellation ID.
        /// </summary>
        public ConstellationView GetConstellationView(int constellationId)
        {
            _constellationViews.TryGetValue(constellationId, out var view);
            return view;
        }
    }
}
