using Unity.Netcode;
using UnityEngine;
using Gameplay.Map.Visualization;

namespace Gameplay.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapGenerator _mapGenerator;
        [SerializeField] private MapVisualizer _mapVisualizer;

        private void Start()
        {
            // Генерация карты должна происходить только на сервере/хосте
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && networkManager.IsServer)
            {
                _mapGenerator?.GenerateMap();
                // Небольшая задержка для синхронизации сетевых объектов
                Invoke(nameof(InitializeVisualization), 0.1f);
            }
            else if (networkManager == null)
            {
                // Если NetworkManager нет (например, в тестах), генерируем локально
                _mapGenerator?.GenerateMap();
                InitializeVisualization();
            }
            else
            {
                // Клиент - ждём пока сервер создаст объекты
                // Визуализация будет инициализирована когда объекты появятся
                StartCoroutine(WaitForMapAndInitialize());
            }
        }

        private void InitializeVisualization()
        {
            if (_mapVisualizer != null)
            {
                _mapVisualizer.Initialize();
            }
            else
            {
                Debug.LogWarning("[MapController] MapVisualizer reference not set!");
            }
        }

        private System.Collections.IEnumerator WaitForMapAndInitialize()
        {
            // Ждём пока GameMap заполнится данными (сетевые объекты синхронизируются)
            var gameMap = _mapGenerator?.GetComponent<GameMap>();
            if (gameMap == null)
            {
                gameMap = FindFirstObjectByType<GameMap>();
            }

            float timeout = 10f;
            float elapsed = 0f;

            while ((gameMap == null || gameMap.Size() == 0) && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;

                if (gameMap == null)
                {
                    gameMap = FindFirstObjectByType<GameMap>();
                }
            }

            if (gameMap != null && gameMap.Size() > 0)
            {
                InitializeVisualization();
            }
            else
            {
                Debug.LogError("[MapController] Timeout waiting for map data on client!");
            }
        }
    }
}
