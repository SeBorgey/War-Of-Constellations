using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;

        private void Start()
        {
            // Генерация карты должна происходить только на сервере/хосте
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsServer)
            {
                _mapGenerator?.GenerateMap();
            }
            else if (networkManager == null)
            {
                // Если NetworkManager нет (например, в тестах), генерируем локально
                _mapGenerator?.GenerateMap();
            }
        }
    }
}
