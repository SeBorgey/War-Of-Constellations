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
            Debug.Log("[ClickInputHandler] Start called");

            // Инициализируем камеру
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    _mainCamera = FindFirstObjectByType<Camera>();
                }
            }

            // PlayerController будет найден динамически в Update, когда игрок заспавнится
        }

        private void Update()
        {
            // Проверяем, что сеть запущена
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || (!networkManager.IsClient && !networkManager.IsServer))
            {
                return;
            }

            // Ищем PlayerController если ещё не найден
            if (_localPlayer == null)
            {
                FindLocalPlayer();
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        private void FindLocalPlayer()
        {
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsClient)
            {
                return;
            }

            // Определяем цвет локального игрока: Host = Blue, Client = Red
            Player localColor = networkManager.IsHost ? Player.Blue : Player.Red;

            var allControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var controller in allControllers)
            {
                // Находим контроллер с нужным цветом
                if (controller.PlayerColor == localColor)
                {
                    _localPlayer = controller;
                    Debug.Log($"[ClickInputHandler] Found local PlayerController: {controller.PlayerColor}");
                    break;
                }
            }
        }

        private void HandleClick()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
            }

            if (_mainCamera == null)
            {
                Debug.LogWarning("[ClickInputHandler] Camera is null!");
                return;
            }

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

            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || (!networkManager.IsClient && !networkManager.IsServer))
            {
                return;
            }

            int power = _localPlayer.GetClickPower();
            Player player = _localPlayer.PlayerColor;

            Debug.Log($"[ClickInputHandler] Player {player} clicked on Star {star.Id} with power {power}");
            star.ClickStarServerRpc(player, power);
        }
    }
}
