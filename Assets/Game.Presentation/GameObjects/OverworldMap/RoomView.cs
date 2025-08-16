using Game.Application.Messaging;
using Game.Domain.Entities.Overworld;
using UnityEngine;
using Game.Core;
using Game.Application.Interfaces;

namespace Game.Presentation.GameObjects.OverworldMap
{
    public class RoomView : MonoObject<RoomEntity>
    {
        public float highlightMultiplier = 1.2f;
        public float fadeSpeed = 8f;

        public Vector3 originalPosition;
        private SpriteRenderer _spriteRenderer;
        private IEventBus _eventBus;
        private IOverworldService _overworldService;
        private Color _originalColor;
        private Color _targetColor;
        private Color _greyedOutColor;
        private bool _isHovering;
        private bool _isAccessible = true;
        void Awake()
        {
            Debug.Log("Room awake");
            originalPosition = transform.position;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _overworldService = ServiceLocator.Get<IOverworldService>();
            _originalColor = _spriteRenderer.color;
            _greyedOutColor = new Color(_originalColor.r * 0.5f, _originalColor.g * 0.5f, _originalColor.b * 0.5f, _originalColor.a * 0.7f);
            _targetColor = _originalColor;
        }

        void Update()
        {
            _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, _targetColor, Time.deltaTime * fadeSpeed);
        }
        
        void Start()
        {
            UpdateAccessibility();
        }


        protected override void OnModelBound()
        {
            Debug.Log("Binding Room");
            UpdateAccessibility();
        }
        
        private void UpdateAccessibility()
        {
            if (model != null && _overworldService != null)
            {
                _isAccessible = _overworldService.IsRoomAccessible(model.Id);
                
                if (!_isAccessible)
                {
                    _targetColor = _greyedOutColor;
                }
                else
                {
                    _targetColor = _originalColor;
                }
            }
        }

        void OnMouseUp()
        {
            if (_isHovering && _isAccessible)
            {
                Debug.Log($"Room spawned: {model} with gui {model?.Id}");
                _eventBus.Publish(new EnterRoomCommand(model.Id));
            }
        }

        void OnMouseEnter()
        {
            _isHovering = true;
            
            if (_isAccessible)
            {
                _targetColor = _originalColor * highlightMultiplier;
                SetPointerCursor();
            }
        }

        void OnMouseExit()
        {
            _isHovering = false;
            
            if (_isAccessible)
            {
                _targetColor = _originalColor;
            }
            else
            {
                _targetColor = _greyedOutColor;
            }
            
            ResetCursor();
        }

        private void SetPointerCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void ResetCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        public void RefreshAccessibility()
        {
            UpdateAccessibility();
        }
    }
}