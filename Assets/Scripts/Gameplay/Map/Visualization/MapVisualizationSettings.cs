using UnityEngine;

namespace Gameplay.Map.Visualization
{
    /// <summary>
    /// ScriptableObject containing all visual settings for map rendering.
    /// </summary>
    [CreateAssetMenu(fileName = "MapVisualizationSettings", menuName = "Game/Map Visualization Settings")]
    public class MapVisualizationSettings : ScriptableObject
    {
        [Header("Star Colors")]
        [Tooltip("Color for neutral (uncaptured) stars")]
        public Color neutralStarColor = Color.white;

        [Tooltip("Color for stars fully captured by Blue player")]
        public Color blueStarColor = new Color(0.3f, 0.5f, 1f);

        [Tooltip("Color for stars fully captured by Red player")]
        public Color redStarColor = new Color(1f, 0.3f, 0.3f);

        [Tooltip("Tint color when Blue is progressing capture")]
        public Color blueTintColor = new Color(0.7f, 0.8f, 1f);

        [Tooltip("Tint color when Red is progressing capture")]
        public Color redTintColor = new Color(1f, 0.7f, 0.7f);

        [Header("Star Sizes")]
        [Tooltip("Base scale multiplier for stars")]
        public float baseStarScale = 0.5f;

        [Tooltip("Scale increment per size level (1-5)")]
        public float scalePerSizeLevel = 0.2f;

        [Tooltip("Pulse animation speed")]
        public float pulseSpeed = 2f;

        [Tooltip("Maximum pulse amplitude")]
        public float pulseAmplitude = 0.1f;

        [Header("Constellation Edges (Internal)")]
        [Tooltip("Color for edges within a neutral constellation")]
        public Color neutralEdgeColor = new Color(1f, 1f, 1f, 0.3f);

        [Tooltip("Color for edges within a Blue-owned constellation")]
        public Color blueEdgeColor = new Color(0.3f, 0.5f, 1f, 0.5f);

        [Tooltip("Color for edges within a Red-owned constellation")]
        public Color redEdgeColor = new Color(1f, 0.3f, 0.3f, 0.5f);

        [Tooltip("Width of constellation internal edges")]
        public float constellationEdgeWidth = 0.5f;

        [Header("Map Edges (Between Constellations)")]
        [Tooltip("Color for edges between constellations")]
        public Color mapEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        [Tooltip("Width of inter-constellation edges")]
        public float mapEdgeWidth = 0.3f;

        [Header("Sprites")]
        [Tooltip("Default sprite for stars (circle)")]
        public Sprite starSprite;

        /// <summary>
        /// Get the color for a star based on its state and capture progress.
        /// </summary>
        public Color GetStarColor(StarState state, float blueProgress, float redProgress)
        {
            switch (state)
            {
                case StarState.Blue:
                    return blueStarColor;
                case StarState.Red:
                    return redStarColor;
                default:
                    if (blueProgress > redProgress && blueProgress > 0)
                    {
                        return Color.Lerp(neutralStarColor, blueTintColor, blueProgress);
                    }
                    else if (redProgress > blueProgress && redProgress > 0)
                    {
                        return Color.Lerp(neutralStarColor, redTintColor, redProgress);
                    }
                    return neutralStarColor;
            }
        }

        /// <summary>
        /// Get the edge color for a constellation based on its owner.
        /// </summary>
        public Color GetConstellationEdgeColor(StarState owner)
        {
            switch (owner)
            {
                case StarState.Blue:
                    return blueEdgeColor;
                case StarState.Red:
                    return redEdgeColor;
                default:
                    return neutralEdgeColor;
            }
        }

        /// <summary>
        /// Calculate the scale for a star based on its size level.
        /// </summary>
        public float GetStarScale(int sizeLevel)
        {
            return baseStarScale + (sizeLevel * scalePerSizeLevel);
        }
    }
}
