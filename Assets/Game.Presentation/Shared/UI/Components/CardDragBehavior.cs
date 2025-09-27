using UnityEngine;
using UnityEngine.EventSystems;
using Game.Presentation.Shared.Views;
using DG.Tweening;
using System.Linq;
using Game.Domain.Enums;

namespace Game.Presentation.Shared.UI.Components
{


    public class CardDragBehavior : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private CardView _cardView;
        private BackpackWindow _backpackWindow;
        private CardOriginType _originType;
        private Vector3 _originalPosition;
        private int _originalSiblingIndex;
        private Transform _originalParent;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private bool _isDragging = false;

        // Visual feedback settings
        private const float DRAG_ALPHA = 0.6f;
        private const float DRAG_SCALE = 1.1f;
        private const float NORMAL_ALPHA = 1.0f;
        private const float ANIMATION_DURATION = 0.2f;

        public void Initialize(CardView cardView, BackpackWindow backpackWindow, CardOriginType originType)
        {
            _cardView = cardView;
            _backpackWindow = backpackWindow;
            _originType = originType;

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_cardView == null) return;

            _isDragging = true;

            // Store original state
            _originalPosition = _rectTransform.localPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();
            _originalParent = transform.parent;

            // Visual feedback for drag start
            transform.SetAsLastSibling(); // Bring to front
            _canvasGroup.alpha = DRAG_ALPHA;
            transform.DOScale(Vector3.one * BackpackWindow.CARD_SCALE * DRAG_SCALE, ANIMATION_DURATION)
                .SetEase(Ease.OutBack);

            // Disable raycast blocking so we can detect drop targets
            _canvasGroup.blocksRaycasts = false;

            // Notify backpack window that drag started
            _backpackWindow.OnCardDragStarted(_cardView, _originType);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            // Follow mouse cursor
            _rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            Debug.Log($"OnEndDrag called for {_cardView.name} ({_originType})");

            _isDragging = false;

            // Restore raycast blocking
            _canvasGroup.blocksRaycasts = true;

            // Check if we dropped on a valid target
            var dropTarget = GetDropTarget(eventData);
            bool validDrop = _backpackWindow.OnCardDropped(_cardView, _originType, dropTarget);

            if (!validDrop)
            {
                Debug.Log($"Invalid drop for {_cardView.name}, returning to original position");
                // Return to original position with animation
                ReturnToOriginalPosition();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDragging) return;

            // Store current sibling index for hover
            int currentSiblingIndex = transform.GetSiblingIndex();

            // Only apply hover effect if not currently being dragged
            transform.DOScale(Vector3.one * BackpackWindow.CARD_SCALE * 2.0f, 0.2f).SetEase(Ease.OutBack);
            transform.SetAsLastSibling();

            // Ensure alpha stays normal during hover
            _canvasGroup.alpha = NORMAL_ALPHA;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDragging) return;

            // Restore normal scale
            transform.DOScale(Vector3.one * BackpackWindow.CARD_SCALE, 0.2f).SetEase(Ease.OutBack);

            // Don't restore sibling index to original, just move back from last
            // This prevents conflicts with the original index stored during initialization

            // Ensure alpha stays normal
            _canvasGroup.alpha = NORMAL_ALPHA;
        }

        private CardView GetDropTarget(PointerEventData eventData)
        {
            // Raycast to find what we're hovering over
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            Debug.Log($"Raycast found {results.Count} results at position {eventData.position}");

            foreach (var result in results)
            {
                Debug.Log($"Raycast hit: {result.gameObject.name} with components: {string.Join(", ", result.gameObject.GetComponents<Component>().Select(c => c.GetType().Name))}");

                // Look for CardView component directly
                var cardView = result.gameObject.GetComponent<CardView>();
                if (cardView != null)
                {
                    // Make sure it's not the card we're dragging
                    var cardDragBehavior = result.gameObject.GetComponent<CardDragBehavior>();
                    if (cardDragBehavior != null && cardDragBehavior != this)
                    {
                        Debug.Log($"Found valid drop target: {cardView.name}");
                        return cardView;
                    }
                }

                // Also check parent objects for CardView (in case we hit a child element)
                var parentCardView = result.gameObject.GetComponentInParent<CardView>();
                if (parentCardView != null)
                {
                    var parentDragBehavior = parentCardView.GetComponent<CardDragBehavior>();
                    if (parentDragBehavior != null && parentDragBehavior != this)
                    {
                        Debug.Log($"Found valid drop target in parent: {parentCardView.name}");
                        return parentCardView;
                    }
                }
            }

            Debug.Log("No valid drop target found");
            return null;
        }

        public void ReturnToOriginalPosition()
        {
            // Animate back to original position
            _rectTransform.DOLocalMove(_originalPosition, ANIMATION_DURATION).SetEase(Ease.OutBack);
            transform.DOScale(Vector3.one * BackpackWindow.CARD_SCALE, ANIMATION_DURATION).SetEase(Ease.OutBack);
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, NORMAL_ALPHA, ANIMATION_DURATION);

            // Restore original sibling index after animation
            DOVirtual.DelayedCall(ANIMATION_DURATION, () => {
                if (this != null)
                {
                    transform.SetSiblingIndex(_originalSiblingIndex);
                }
            });
        }

        public void AnimateToPosition(Vector3 targetPosition, System.Action onComplete = null)
        {
            Debug.Log($"AnimateToPosition called for {_cardView.name}: from {_rectTransform.localPosition} to {targetPosition}");

            // Animate to new position (used for successful swaps)
            _rectTransform.DOLocalMove(targetPosition, ANIMATION_DURATION).SetEase(Ease.OutBack);
            transform.DOScale(Vector3.one * BackpackWindow.CARD_SCALE, ANIMATION_DURATION).SetEase(Ease.OutBack);
            DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, NORMAL_ALPHA, ANIMATION_DURATION);

            // Update the original position to the new target position for future drags
            _originalPosition = targetPosition;

            if (onComplete != null)
            {
                DOVirtual.DelayedCall(ANIMATION_DURATION, () => onComplete?.Invoke());
            }
        }

        public CardOriginType GetOriginType()
        {
            return _originType;
        }

        public void SetOriginType(CardOriginType newOriginType)
        {
            _originType = newOriginType;
        }

        public Vector3 GetOriginalPosition()
        {
            return _originalPosition;
        }

        private void OnDestroy()
        {
            // Clean up any running tweens
            transform.DOKill();
            _canvasGroup?.DOKill();
        }
    }
}