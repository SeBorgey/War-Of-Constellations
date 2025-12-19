using UnityEngine;

namespace Gameplay.Map
{
    public enum MapComplexity
    {
        Small,
        Normal,
        Big,
        Vast
    }

    [System.Serializable]
    public class MapComplexityConfig(MapComplexity complexity, int constellations, Color color)
    {
        public MapComplexity complexity = complexity;
        public int constellationsCount = constellations;
        public Color labelColor = color;

        public static MapComplexityConfig GetConfig(MapComplexity complexity)
        {
            return complexity switch
            {
                MapComplexity.Small => new MapComplexityConfig(MapComplexity.Small, 3, Color.green),
                MapComplexity.Normal => new MapComplexityConfig(MapComplexity.Normal, 5, Color.yellow),
                MapComplexity.Big => new MapComplexityConfig(MapComplexity.Big, 7, new Color(1f, 0.5f, 0f)),// Orange
                MapComplexity.Vast => new MapComplexityConfig(MapComplexity.Vast, 10, Color.red),
                _ => new MapComplexityConfig(MapComplexity.Normal, 5, Color.yellow),
            };
        }

        // Weighted star count distribution
        // Min: 3, Default: 5, Max: 10
        // Distribution: 3-5 (common), 6-7 (uncommon), 8 (rare ~3%), 9-10 (very rare ~1% each)
        public static int GetRandomStarCount()
        {
            float roll = Random.Range(0f, 100f);

            // Cumulative weights (total = 100%)
            if (roll < 20f)       return 3;  // 20%
            else if (roll < 50f)  return 4;  // 30%
            else if (roll < 80f)  return 5;  // 30% (default/most common)
            else if (roll < 90f)  return 6;  // 10%
            else if (roll < 95f)  return 7;  // 5%
            else if (roll < 98f)  return 8;  // 3%
            else if (roll < 99f)  return 9;  // 1%
            else                  return 10; // 1%
        }
    }
}
