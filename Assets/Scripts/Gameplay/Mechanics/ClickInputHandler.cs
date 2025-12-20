using Unity.Netcode;
using UnityEngine;
using Gameplay.Map;
using System.IO;

namespace Gameplay.Mechanics
{
    public class ClickInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController _localPlayer;
        [SerializeField] private Camera _mainCamera;
        private const string LOG_PATH = "/home/ivan/Desktop/unity/War-Of-Constellations/.cursor/debug.log";

        private void Start()
        {
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClickInputHandler.cs:Start\",\"message\":\"ClickInputHandler Start called\",\"data\":{{\"active\":{gameObject.activeSelf},\"enabled\":{enabled}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            
            // Инициализируем камеру
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    // Пытаемся найти любую камеру в сцене
                    _mainCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
                    // #region agent log
                    try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"ClickInputHandler.cs:Start\",\"message\":\"Camera search\",\"data\":{{\"cameraMain\":{(Camera.main != null).ToString().ToLower()},\"cameraFound\":{(_mainCamera != null).ToString().ToLower()},\"cameraName\":\"{(_mainCamera != null ? _mainCamera.name : "null")}\"}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                    // #endregion
                }
            }
            
            if (_localPlayer == null)
            {
                _localPlayer = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"ClickInputHandler.cs:Start\",\"message\":\"PlayerController search result\",\"data\":{{\"found\":{(_localPlayer != null).ToString().ToLower()}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
            }
            else
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"ClickInputHandler.cs:Start\",\"message\":\"PlayerController already assigned\",\"data\":{{\"found\":true}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
            }
        }

        private void Update()
        {
            // Проверяем, что клиент подключен к сети
            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager != null && !networkManager.IsClient)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"F\",\"location\":\"ClickInputHandler.cs:Update\",\"message\":\"Network check blocked click\",\"data\":{{\"networkManagerExists\":{(networkManager != null).ToString().ToLower()},\"isClient\":{(networkManager != null && networkManager.IsClient).ToString().ToLower()}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                return; // Не обрабатываем клики, если не подключены к сети
            }

            if (Input.GetMouseButtonDown(0))
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"ClickInputHandler.cs:Update\",\"message\":\"Mouse click detected\",\"data\":{{\"mousePos\":\"{Input.mousePosition}\"}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                HandleClick();
            }
        }

        private void HandleClick()
        {
            // Пытаемся найти камеру, если она не установлена
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    // Пытаемся найти любую камеру в сцене
                    _mainCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
                }
            }
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"ClickInputHandler.cs:HandleClick\",\"message\":\"Camera check\",\"data\":{{\"cameraExists\":{(_mainCamera != null).ToString().ToLower()},\"cameraName\":\"{(_mainCamera != null ? _mainCamera.name : "null")}\"}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

            // Проверяем, что камера существует перед использованием
            if (_mainCamera == null)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"C\",\"location\":\"ClickInputHandler.cs:HandleClick\",\"message\":\"Camera is null, aborting click\",\"data\":{{}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Debug.LogWarning("[ClickInputHandler] Main camera is null! Cannot process click. Make sure there is a Camera in the scene with tag 'MainCamera'.");
                return;
            }

            // Используем 2D Raycast для клика по узлам
            Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"ClickInputHandler.cs:HandleClick\",\"message\":\"World position calculated\",\"data\":{{\"screenPos\":\"{Input.mousePosition}\",\"worldPos\":\"{mousePos}\"}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"ClickInputHandler.cs:HandleClick\",\"message\":\"OverlapPoint result\",\"data\":{{\"hitFound\":{(hit != null).ToString().ToLower()},\"hitName\":\"{(hit != null ? hit.name : "null")}\",\"hitTag\":\"{(hit != null ? hit.tag : "null")}\"}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (hit != null)
            {
                var star = hit.GetComponent<Star>();
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"D\",\"location\":\"ClickInputHandler.cs:HandleClick\",\"message\":\"Star component check\",\"data\":{{\"starFound\":{(star != null).ToString().ToLower()},\"starId\":{(star != null ? star.Id : -1)}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                if (star != null)
                {
                    OnStarClicked(star);
                }
            }
        }

        private void OnStarClicked(Star star)
        {
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"G\",\"location\":\"ClickInputHandler.cs:OnStarClicked\",\"message\":\"OnStarClicked called\",\"data\":{{\"starId\":{star.Id}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (_localPlayer == null)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"E\",\"location\":\"ClickInputHandler.cs:OnStarClicked\",\"message\":\"LocalPlayer is null\",\"data\":{{}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Debug.LogWarning("[ClickInputHandler] LocalPlayer is null!");
                return;
            }

            var networkManager = Unity.Netcode.NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsClient)
            {
                // #region agent log
                try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"F\",\"location\":\"ClickInputHandler.cs:OnStarClicked\",\"message\":\"Network check failed\",\"data\":{{\"networkManagerExists\":{(networkManager != null).ToString().ToLower()},\"isClient\":{(networkManager != null && networkManager.IsClient).ToString().ToLower()}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                Debug.LogWarning("[ClickInputHandler] Not connected to network!");
                return;
            }

            int power = _localPlayer.GetClickPower();
            Player player = _localPlayer.PlayerColor;
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"G\",\"location\":\"ClickInputHandler.cs:OnStarClicked\",\"message\":\"Before ServerRpc call\",\"data\":{{\"player\":\"{player}\",\"power\":{power},\"starId\":{star.Id}}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion

            Debug.Log($"[ClickInputHandler] Player {player} clicked on Star {star.Id} at {star.Coordinates} with power {power}");

            // Отправляем клик на сервер через ServerRpc
            star.ClickStarServerRpc(player, power);
            // #region agent log
            try { File.AppendAllText(LOG_PATH, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"G\",\"location\":\"ClickInputHandler.cs:OnStarClicked\",\"message\":\"After ServerRpc call\",\"data\":{{}},\"timestamp\":{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
        }
    }
}
