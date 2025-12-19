using Unity.Netcode;
using UnityEngine;
using Gameplay.Map;

namespace Gameplay.Mechanics
{
    public class ClickInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController _localPlayer;
        [SerializeField] private Camera _mainCamera;

        private void Start()
        {
            if (_localPlayer == null)
            {
                _localPlayer = FindObjectOfType<PlayerController>();
            }
        }

        private void Update()
        {
            // Проверяем, что клиент подключен к сети
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && !networkManager.IsClient)
            {
                return; // Не обрабатываем клики, если не подключены к сети
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        private void HandleClick()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // Используем 2D Raycast для клика по узлам
            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null)
            {
                var star = hit.GetComponent<Star>();
                if (star != null)
                {
                    OnStarClicked(star);
                }
            }
        }

        private void OnStarClicked(Star star)
        {
            if (_localPlayer == null)
            {
                Debug.LogWarning("[ClickInputHandler] LocalPlayer is null!");
                return;
            }

            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsClient)
            {
                Debug.LogWarning("[ClickInputHandler] Not connected to network!");
                return;
            }

            int power = _localPlayer.GetClickPower();
            Player player = _localPlayer.PlayerColor;

            Debug.Log($"[ClickInputHandler] Player {player} clicked on Star {star.Id} at {star.Coordinates} with power {power}");

            // Отправляем клик на сервер через ServerRpc
            star.ClickStarServerRpc(player, power);
        }
    }
}
