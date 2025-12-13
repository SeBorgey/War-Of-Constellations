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
                var node = hit.collider.GetComponent<Node>();
                if (node != null)
                {
                    OnNodeClicked(node);
                }
            }
        }

        private void OnNodeClicked(Node node)
        {
            if (_localPlayer == null) return;
            
            float power = _localPlayer.GetClickPower();
            
            // TODO: Send input to server/host
            Debug.Log($"Clicked on Node {node.Id} with power {power}");
            
            // For now (direct interaction stub):
            // if (node.OwnerId == _localPlayer.PlayerId) 
            //    node.ChangeValue(power);
            // else 
            //    node.ChangeValue(-power);
        }
    }
}
