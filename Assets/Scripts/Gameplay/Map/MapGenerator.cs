using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _numberOfConstellations = 5;
        [SerializeField] private int _nodesPerConstellation = 10;
        
        [Header("References")]
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private Transform _container;

        public void GenerateMap()
        {
            Debug.Log("Generating Map...");
            // TODO: Implement procedural generation of graph
        }
    }
}
