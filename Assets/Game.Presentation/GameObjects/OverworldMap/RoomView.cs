using Game.Application.Messaging;
using Game.Domain.Entities.Overworld;
using UnityEngine;
using Game.Core;

namespace Game.Presentation.GameObjects.OverworldMap
{
    public class RoomView : MonoObject<RoomEntity>
    {
        public float highlightMultiplier = 1.2f;
        public float fadeSpeed = 8f;

        public Vector3 originalPosition;
        private SpriteRenderer _spriteRenderer;
        private IEventBus _eventBus;
        private Color _originalColor;
        private Color _targetColor;
        private bool _isHovering;
        void Awake()
        {
            Debug.Log("Room awake");
            originalPosition = transform.position;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _originalColor = _spriteRenderer.color;
            _targetColor = _originalColor;
        }

        void Update()
        {
            _spriteRenderer.color = Color.Lerp(_spriteRenderer.color, _targetColor, Time.deltaTime * fadeSpeed);
        }


        protected override void OnModelBound()
        {
            Debug.Log("Binding Room");
        }

        void OnMouseUp()
        {
            if (_isHovering)
            {
                Debug.Log($"Room spawned: {model} with gui {model?.Id}");
                _eventBus.Publish(new EnterRoomCommand(model.Id));
            }
        }

        void OnMouseEnter()
        {
            _isHovering = true;
            _targetColor = _originalColor * highlightMultiplier;
            SetPointerCursor();
        }

        void OnMouseExit()
        {
            _isHovering = false;
            _targetColor = _originalColor;
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
    }
}