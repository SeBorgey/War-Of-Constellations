using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkCursor : NetworkBehaviour
    {
        private readonly NetworkVariable<Vector2> _position = new NetworkVariable<Vector2>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (IsOwner)
            {
                UpdateOwnerPosition();
                CheckClickInput();
            }

            transform.position = _position.Value;
        }

        private void UpdateOwnerPosition()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -_mainCamera.transform.position.z;
            _position.Value = _mainCamera.ScreenToWorldPoint(mousePos);
        }

        private void CheckClickInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SubmitClickServerRpc(_position.Value);
            }
        }

        [ServerRpc]
        private void SubmitClickServerRpc(Vector2 position)
        {
            Debug.Log($"Player {OwnerClientId} clicked at {position}");
        }
    }
}
