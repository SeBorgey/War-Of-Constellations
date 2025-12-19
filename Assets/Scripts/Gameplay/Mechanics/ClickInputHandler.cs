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

            float power = _localPlayer.GetClickPower();

            // TODO: Send input to server/host
            Debug.Log($"Clicked on Star at {star.Coordinates} with power {power}");

            // For now (direct interaction stub):
            // if (star.Owner == _localPlayer.PlayerId)
            //    star.ChangeValue(power);
            // else
            //    star.ChangeValue(-power);
        }
    }
}
