using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using Game.Application.Messaging;
using Game.Core;
using Game.Presentation.Models;

namespace Game.Presentation.UI.ButtonUI
{
    [AddComponentMenu("UI/Button UI (with events)")]
    [DisallowMultipleComponent]
    public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Label Settings")]
        [SerializeField] private ButtonLabel _textObject;
        public string inputText = "Click Me";

        [Header("Button Config")]
        [SubclassOf(typeof(ICommand))]
        public CommandEntry[] messages;
        private event Action OnClick;
        // refs
        private IEventBus _eventBus;
        private Button _clickButton;
        private bool _needsRebuild;
        
        // Hover effect settings
        private Vector3 _originalScale = Vector3.one;
        private float _hoverScale = 1.05f;
        private float _hoverAnimationDuration = 0.1f;
        private Coroutine _currentHoverAnimation;
        
        /// <summary>
        /// Add a listener to the button click event (similar to Unity's Button.onClick.AddListener)
        /// </summary>
        public void AddListener(System.Action action)
        {
            OnClick += action;
        }
        
        /// <summary>
        /// Remove a listener from the button click event
        /// </summary>
        public void RemoveListener(System.Action action)
        {
            OnClick -= action;
        }
        
        /// <summary>
        /// Remove all listeners from the button click event
        /// </summary>
        public void RemoveAllListeners()
        {
            OnClick = null;
        }

        private void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            
            // Store original scale
            _originalScale = transform.localScale;
            
            // Get or add Unity Button component
            _clickButton = GetComponent<Button>();
            if (_clickButton == null)
                _clickButton = gameObject.AddComponent<Button>();
                
            // Setup click listener
            _clickButton.onClick.RemoveAllListeners();
            _clickButton.onClick.AddListener(OnClickFeedback);
        }

        private void Start()
        {
            SetText(inputText);
        }

        public void SetText(string text)
        {
            if (_textObject == null)
            {
                return;
            }
            _textObject.SetText(text);
        }

        private void OnClickFeedback()
        {
            if (!UnityEngine.Application.isPlaying) return;

            // Invoke custom listeners
            OnClick?.Invoke();

            // Execute command messages
            foreach (var message in messages)
            {
                Debug.Log("ButtonUI: Clicked!");
                var commandAsset = (ICommandAsset)message.commandAsset;
                if (commandAsset == null) return;
                Debug.Log($"ButtonUI: Publishing command {commandAsset.GetType().Name}");

                _eventBus?.Publish((dynamic)commandAsset.Create());
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentHoverAnimation != null)
                StopCoroutine(_currentHoverAnimation);
                
            _currentHoverAnimation = StartCoroutine(AnimateScale(_originalScale * _hoverScale));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_currentHoverAnimation != null)
                StopCoroutine(_currentHoverAnimation);
                
            _currentHoverAnimation = StartCoroutine(AnimateScale(_originalScale));
        }

        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < _hoverAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _hoverAnimationDuration;
                
                // Use smooth step for eased animation
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
                yield return null;
            }
            
            transform.localScale = targetScale;
            _currentHoverAnimation = null;
        }

    }
}
