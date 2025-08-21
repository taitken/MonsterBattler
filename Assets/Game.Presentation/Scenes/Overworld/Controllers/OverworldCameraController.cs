using UnityEngine;

namespace Game.Presentation.Controllers
{
    public class OverworldCameraController : MonoBehaviour
    {
        [SerializeField] private float dragSensitivity = 2f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private SpriteRenderer backgroundImage;
        
        private float _minX, _maxX;
        private bool _boundsCalculated = false;
        
        
        private Camera _camera;
        private Vector3 _lastMousePosition;
        private Vector3 _targetPosition;
        private Vector3 _velocity = Vector3.zero;
        private bool _isDragging = false;
        

        void Start()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            _targetPosition = transform.position;
            CalculateSpriteBounds();
        }

        void Update()
        {
            HandleMouseInput();
            SmoothCameraMovement();
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
                
                // Only allow horizontal (X-axis) movement
                _targetPosition.x += deltaMovement.x * dragSensitivity;
                
                // Apply sprite bounds if calculated
                if (_boundsCalculated)
                {
                    _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
                }
                
                _lastMousePosition = currentMousePosition;
            }
            
            // Stop dragging on left mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
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

        // Optional method to reset camera to original position
        public void ResetCamera()
        {
            _targetPosition = Vector3.zero;
        }

        // Optional method to focus on a specific point (X-axis only)
        public void FocusOn(Vector3 position)
        {
            _targetPosition = new Vector3(position.x, _targetPosition.y, transform.position.z);
            
            // Apply bounds when focusing
            if (_boundsCalculated)
            {
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
            }
        }
        
        private void CalculateSpriteBounds()
        {
            if (backgroundImage == null || _camera == null)
            {
                _boundsCalculated = false;
                return;
            }
            
            // Get the sprite bounds in world space
            Bounds spriteBounds = backgroundImage.bounds;
            
            // Calculate camera's half-width at current orthographic size
            float cameraHalfWidth = _camera.orthographicSize * _camera.aspect;
            
            // Set bounds so camera can pan to see the edges of the sprite
            // But prevent camera from going beyond the sprite edges
            _minX = spriteBounds.min.x + cameraHalfWidth;
            _maxX = spriteBounds.max.x - cameraHalfWidth;
            
            // Ensure minX doesn't exceed maxX (in case sprite is smaller than camera view)
            if (_minX > _maxX)
            {
                float center = (spriteBounds.min.x + spriteBounds.max.x) / 2f;
                _minX = _maxX = center;
            }
            
            _boundsCalculated = true;
        }
        
        // Call this if the background sprite changes or camera settings change
        public void RecalculateBounds()
        {
            CalculateSpriteBounds();
        }
    }
}