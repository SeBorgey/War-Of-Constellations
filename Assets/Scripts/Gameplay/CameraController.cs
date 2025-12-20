using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Simple camera controller with zoom (scroll wheel) and pan (arrow keys/WASD/drag).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSpeed = 50f;
        [SerializeField] private float _minZoom = 50f;
        [SerializeField] private float _maxZoom = 1000f;

        [Header("Pan Settings")]
        [SerializeField] private float _panSpeed = 500f;
        [SerializeField] private float _dragSpeed = 2f;

        [Header("Bounds (optional)")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private float _boundsX = 1500f;
        [SerializeField] private float _boundsY = 1000f;

        private Camera _camera;
        private Vector3 _dragOrigin;
        private bool _isDragging;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            HandleZoom();
            HandleKeyboardPan();
            HandleMouseDrag();
            ClampPosition();
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float newSize = _camera.orthographicSize - scroll * _zoomSpeed;
                _camera.orthographicSize = Mathf.Clamp(newSize, _minZoom, _maxZoom);
            }
        }

        private void HandleKeyboardPan()
        {
            float horizontal = 0f;
            float vertical = 0f;

            // Arrow keys and WASD
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                horizontal = 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                vertical = -1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                vertical = 1f;

            if (horizontal != 0f || vertical != 0f)
            {
                // Scale pan speed with zoom level
                float scaledSpeed = _panSpeed * (_camera.orthographicSize / 200f);
                Vector3 move = new Vector3(horizontal, vertical, 0f) * scaledSpeed * Time.deltaTime;
                transform.position += move;
            }
        }

        private void HandleMouseDrag()
        {
            // Middle mouse button or right mouse button for drag
            if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
            {
                _dragOrigin = _camera.ScreenToWorldPoint(Input.mousePosition);
                _isDragging = true;
            }

            if (Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 currentPos = _camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = _dragOrigin - currentPos;
                transform.position += difference;
            }
        }

        private void ClampPosition()
        {
            if (!_useBounds) return;

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -_boundsX, _boundsX);
            pos.y = Mathf.Clamp(pos.y, -_boundsY, _boundsY);
            transform.position = pos;
        }

        /// <summary>
        /// Center camera on a specific position.
        /// </summary>
        public void CenterOn(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        /// <summary>
        /// Set zoom level.
        /// </summary>
        public void SetZoom(float size)
        {
            _camera.orthographicSize = Mathf.Clamp(size, _minZoom, _maxZoom);
        }
    }
}
