using UnityEngine;
using Gameplay.Map;

namespace Gameplay.Mechanics
{
    public class ClickInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController _localPlayer;
        [SerializeField] private Camera _mainCamera;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }

        private void HandleClick()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var star = hit.collider.GetComponent<Star>();
                if (star != null)
                {
                    OnStarClicked(star);
                }
            }
        }

        private void OnStarClicked(Star star)
        {
            if (_localPlayer == null) return;

            int power = _localPlayer.GetClickPower();
            Player player = _localPlayer.PlayerColor;

            // TODO: Send input to server/host
            Debug.Log($"Player {player} clicked on Star {star.Id} at {star.Coordinates} with power {power}");

            // For now (direct interaction stub):
            // star.ApplyDamage(player, power);
        }
    }
}
