using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Map
{
    public class Constellation : MonoBehaviour
    {
        [SerializeField] private List<Node> _nodes = new List<Node>();
        [SerializeField] private int _constellationId;

        public void Initialize(int id, List<Node> nodes)
        {
            _constellationId = id;
            _nodes = nodes;
        }

        public bool IsFullyOwnedBy(int playerId)
        {
            foreach (var node in _nodes)
            {
                if (node.OwnerId != playerId)
                    return false;
            }
            return true;
        }
    }
}
