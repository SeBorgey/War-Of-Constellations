using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class Node : MonoBehaviour
    {
        [Header("Node Data")]
        [SerializeField] private int _id;
        [SerializeField] private float _value;
        [SerializeField] private int _ownerId = -1; // -1 for neutral

        [Header("Connections")]
        [SerializeField] private List<Node> _neighbors = new List<Node>();

        public int Id => _id;
        public float Value => _value;
        public int OwnerId => _ownerId;

        public void Initialize(int id, float initialValue)
        {
            _id = id;
            _value = initialValue;
        }

        public void AddNeighbor(Node neighbor)
        {
            if (!_neighbors.Contains(neighbor))
            {
                _neighbors.Add(neighbor);
            }
        }

        public void ChangeValue(float amount)
        {
            _value += amount;
            // TODO: Handle ownership change if value <= 0
        }

        public void SetOwner(int newOwnerId)
        {
            _ownerId = newOwnerId;
            // TODO: Update visual color
        }
    }
}
