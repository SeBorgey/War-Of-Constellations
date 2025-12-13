using UnityEngine;

namespace Gameplay.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;

        private void Start()
        {
            if (_mapGenerator != null)
            {
                _mapGenerator.GenerateMap();
            }
        }
    }
}
