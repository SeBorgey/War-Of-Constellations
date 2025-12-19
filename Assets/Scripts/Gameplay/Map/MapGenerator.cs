using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map Complexity")]
        [SerializeField] private MapComplexity _mapComplexity = MapComplexity.Normal;

        [Header("Map Bounds")]
        [SerializeField] private float _mapWidth = 1920f;
        [SerializeField] private float _mapHeight = 1080f;

        [Header("References")]
        [SerializeField] private GameObject _starPrefab;
        [SerializeField] private GameObject _constellationPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private GameMap _gameMap;

        // Generation parameters
        private const float INITIAL_CONSTELLATION_RADIUS = 300f;
        private const float RADIUS_REDUCTION_FACTOR = 0.75f;
        private const int MAX_PLACEMENT_ATTEMPTS = 10;

        private const float INITIAL_STAR_SPACING = 20f;
        private const float STAR_SPACING_REDUCTION_FACTOR = 0.75f;
        private const float CONSTELLATION_SPREAD_FACTOR = 0.25f; // Distance to closest / 4

        private MapComplexityConfig _config;
        private List<Vector2> _constellationCenters = new List<Vector2>();
        private int _nextStarId;

        public void GenerateMap()
        {
            // Генерация карты должна происходить только на сервере/хосте
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && !networkManager.IsServer)
            {
                Debug.Log("[MapGenerator] Map generation skipped - not server/host. Client will receive map from server.");
                return;
            }

            Debug.Log($"[MapGenerator] Generating Map with complexity: {_mapComplexity}");

            // Get configuration based on complexity
            _config = MapComplexityConfig.GetConfig(_mapComplexity);

            // Clear existing map if any
            _gameMap?.Clear();

            _constellationCenters.Clear();
            _nextStarId = 0;

            // Step 1: Generate constellation centers
            GenerateConstellationCenters();

            // Step 2: Generate stars for each constellation
            GenerateStarsForConstellations();

            // Step 3: Compute constellation neighbors using Delaunay triangulation
            _gameMap?.ComputeConstellationNeighbors();

            Debug.Log($"[MapGenerator] Map generation complete! Created {_gameMap.Size()} constellations.");
        }

        private void GenerateConstellationCenters()
        {
            float currentRadius = INITIAL_CONSTELLATION_RADIUS;
            int targetCount = _config.constellationsCount;

            for (int i = 0; i < targetCount; i++)
            {
                Vector2 newCenter = Vector2.zero;
                bool validPosition = false;
                int attempts = 0;
                int totalAttempts = 0;

                while (!validPosition && totalAttempts < 1000) // Safety limit
                {
                    // Generate random position within map bounds
                    newCenter = new Vector2(
                        Random.Range(-_mapWidth / 2, _mapWidth / 2),
                        Random.Range(-_mapHeight / 2, _mapHeight / 2)
                    );

                    // Check if it's far enough from existing centers
                    validPosition = true;
                    foreach (var existingCenter in _constellationCenters)
                    {
                        if (Vector2.Distance(newCenter, existingCenter) < currentRadius)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    attempts++;
                    totalAttempts++;

                    // Reduce radius if we've tried too many times
                    if (attempts >= MAX_PLACEMENT_ATTEMPTS)
                    {
                        currentRadius *= RADIUS_REDUCTION_FACTOR;
                        attempts = 0;
                        Debug.Log($"Reduced constellation radius to {currentRadius}");
                    }
                }

                if (validPosition)
                {
                    _constellationCenters.Add(newCenter);
                    Debug.Log($"Generated constellation center {i + 1}/{targetCount} at {newCenter}");
                }
                else
                {
                    Debug.LogWarning($"Failed to place constellation {i + 1} after {totalAttempts} attempts");
                }
            }
        }

        private void GenerateStarsForConstellations()
        {
            for (int i = 0; i < _constellationCenters.Count; i++)
            {
                Vector2 center = _constellationCenters[i];

                // Create constellation GameObject
                GameObject constellationObj = _constellationPrefab != null
                    ? Instantiate(_constellationPrefab, _container)
                    : new GameObject($"Constellation_{i}");

                if (_constellationPrefab == null)
                {
                    constellationObj.transform.SetParent(_container);
                }

                Constellation constellation = constellationObj.GetComponent<Constellation>();
                constellation ??= constellationObj.AddComponent<Constellation>();

                constellation.Initialize(i, center);

                // Calculate distance to closest constellation
                float distanceToClosest = GetDistanceToClosestConstellation(center, i);
                float constellationRadius = distanceToClosest * CONSTELLATION_SPREAD_FACTOR;

                // Get star count using weighted distribution (3-10 stars, 5 is most common)
                int starCount = MapComplexityConfig.GetRandomStarCount();

                Debug.Log($"Constellation {i}: radius={constellationRadius}, stars={starCount}, closest={distanceToClosest}");

                // Generate stars for this constellation
                List<Vector2> starPositions = new List<Vector2>();

                // First star is always at the center (primary star)
                CreateStar(constellation, center, Random.Range(3, 6), starPositions);

                // Generate remaining stars
                float currentStarSpacing = INITIAL_STAR_SPACING;

                for (int s = 1; s < starCount; s++)
                {
                    bool validPosition = false;
                    int attempts = 0;
                    int totalAttempts = 0;

                    while (!validPosition && totalAttempts < 100)
                    {
                        // Random distance from center (0 to constellation radius)
                        float distance = Random.Range(0f, constellationRadius);

                        // Random angle (0 to 360 degrees)
                        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                        // Calculate position
                        Vector2 starPosition = center + new Vector2(
                            Mathf.Cos(angle) * distance,
                            Mathf.Sin(angle) * distance
                        );

                        // Check if it's far enough from other stars
                        validPosition = true;
                        foreach (var existingPosition in starPositions)
                        {
                            if (Vector2.Distance(starPosition, existingPosition) < currentStarSpacing)
                            {
                                validPosition = false;
                                break;
                            }
                        }

                        if (validPosition)
                        {
                            // Random star size (1-5)
                            int starSize = Random.Range(1, 6);
                            CreateStar(constellation, starPosition, starSize, starPositions);
                        }

                        attempts++;
                        totalAttempts++;

                        // Reduce spacing requirement if struggling to place
                        if (attempts >= MAX_PLACEMENT_ATTEMPTS)
                        {
                            currentStarSpacing *= STAR_SPACING_REDUCTION_FACTOR;
                            attempts = 0;
                        }
                    }
                }

                // Add constellation to map
                _gameMap?.AddConstellation(constellation);
            }
        }

        private void CreateStar(Constellation constellation, Vector2 position, int size, List<Vector2> starPositions)
        {
            // Создание Star должно происходить только на сервере
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer)
            {
                Debug.LogWarning("[MapGenerator] CreateStar called but not on server!");
                return;
            }

            GameObject starObj = _starPrefab != null
                ? Instantiate(_starPrefab, constellation.transform)
                : new GameObject($"Star_{starPositions.Count}");

            if (_starPrefab == null)
            {
                starObj.transform.SetParent(constellation.transform);
            }

            Star star = starObj.GetComponent<Star>();
            if (star == null)
            {
                star = starObj.AddComponent<Star>();
            }

            // Убеждаемся, что у Star есть NetworkObject компонент
            NetworkObject networkObject = starObj.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                networkObject = starObj.AddComponent<NetworkObject>();
            }

            // Generate unique star ID and random HP (1-20, uniform distribution)
            int starId = _nextStarId++;
            int hp = Random.Range(1, 21);
            star.Initialize(starId, position, size, hp);

            // Спавним Star через сеть
            networkObject.Spawn();

            constellation.AddStar(star);
            starPositions.Add(position);
            
            Debug.Log($"[MapGenerator] Created and spawned Star {starId} at {position} with HP {hp}");
        }

        private float GetDistanceToClosestConstellation(Vector2 center, int excludeIndex)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < _constellationCenters.Count; i++)
            {
                if (i == excludeIndex)
                    continue;

                float distance = Vector2.Distance(center, _constellationCenters[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            // If this is the first constellation, return a default value
            if (minDistance == float.MaxValue)
            {
                minDistance = INITIAL_CONSTELLATION_RADIUS * 2;
            }

            return minDistance;
        }
    }
}
