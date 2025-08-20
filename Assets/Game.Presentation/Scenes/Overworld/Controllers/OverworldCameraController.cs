using UnityEngine;

namespace Game.Presentation.Controllers
{
    public class OverworldCameraController : MonoBehaviour
    {
        [SerializeField] private float dragSensitivity = 2f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private Vector2 panBounds = new Vector2(20f, 20f); // Optional bounds for panning
        [SerializeField] private bool usePanBounds = false;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSensitivity = 1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float zoomSmoothTime = 0.1f;
        
        private Camera _camera;
        private Vector3 _lastMousePosition;
        private Vector3 _targetPosition;
        private Vector3 _velocity = Vector3.zero;
        private bool _isDragging = false;
        
        private float _targetOrthographicSize;
        private float _originalOrthographicSize;
        private float _zoomVelocity = 0f;

        void Start()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            _targetPosition = transform.position;
            _originalOrthographicSize = _camera.orthographicSize;
            _targetOrthographicSize = _originalOrthographicSize;
        }

        void Update()
        {
            HandleMouseInput();
            HandleZoomInput();
            SmoothCameraMovement();
            SmoothZoomMovement();
        }

        private void HandleMouseInput()
        {
            // Start dragging on left mouse button down
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            }
            
            // Continue dragging while left mouse button is held
            if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector3 currentMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 deltaMovement = _lastMousePosition - currentMousePosition;
                
                _targetPosition += deltaMovement * dragSensitivity;
                
                // Apply bounds if enabled
                if (usePanBounds)
                {
                    _targetPosition.x = Mathf.Clamp(_targetPosition.x, -panBounds.x, panBounds.x);
                    _targetPosition.y = Mathf.Clamp(_targetPosition.y, -panBounds.y, panBounds.y);
                }
                
                _lastMousePosition = currentMousePosition;
            }
            
            // Stop dragging on left mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
        }

        private void HandleZoomInput()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                _targetOrthographicSize -= scrollInput * zoomSensitivity;
                
                // Convert zoom factor to orthographic size (inverse relationship)
                float minOrthographicSize = _originalOrthographicSize / maxZoom;
                float maxOrthographicSize = _originalOrthographicSize / minZoom;
                
                _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, minOrthographicSize, maxOrthographicSize);
            }
        }

        private void SmoothCameraMovement()
        {
            // Smoothly move camera to target position
            if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, smoothTime);
            }
        }

        private void SmoothZoomMovement()
        {
            // Smoothly zoom camera to target size
            if (Mathf.Abs(_camera.orthographicSize - _targetOrthographicSize) > 0.01f)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _targetOrthographicSize, ref _zoomVelocity, zoomSmoothTime);
            }
        }

        // Optional method to reset camera to original position
        public void ResetCamera()
        {
            _targetPosition = Vector3.zero;
        }

        // Optional method to focus on a specific point
        public void FocusOn(Vector3 position)
        {
            _targetPosition = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}