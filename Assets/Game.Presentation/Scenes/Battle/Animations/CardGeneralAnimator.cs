using System.Collections;
using Game.Application.DTOs;
using Game.Application.Enums;
using Game.Application.Messaging;
using Game.Presentation.Shared.Views;
using Game.Presentation.Core.Interfaces;
using UnityEngine;
using Game.Application.Interfaces;

namespace Game.Presentation.Scenes.Battle.Animations
{
    public class CardGeneralAnimator : MonoBehaviour
    {
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
        }

        public IEnumerator AnimateGeneralAction(CardView cardView, Vector3 startPosition, Vector3 floatTarget, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;

            // Default floating motion for 2 seconds
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 2f;

                // Gentle floating motion
                float floatOffset = Mathf.Sin(t * 3.14159f * 2) * 0.2f;
                cardTransform.position = Vector3.Lerp(startPosition, floatTarget, t * 0.5f) + new Vector3(0, floatOffset, 0);

                yield return null;
            }

            // Signal hit point for non-attack cards
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.Hit));
        }

        public IEnumerator FadeOutAndDestroy(CardView cardView, int originalSortingOrder)
        {
            if (cardView == null) yield break;

            // Restore original sorting order before destruction
            RestoreCardSortingOrder(cardView, originalSortingOrder);

            var canvasGroup = cardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();

            var elapsed = 0f;
            while (elapsed < 0.5f && cardView != null)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsed / 0.5f);
                yield return null;
            }

            if (cardView != null)
                Destroy(cardView.gameObject);
        }

        public int BringCardToFront(CardView cardView)
        {
            // Try to get Canvas component first (for UI cards)
            var canvas = cardView.GetComponent<Canvas>();
            if (canvas != null)
            {
                int originalOrder = canvas.sortingOrder;
                canvas.sortingOrder = 1000; // High value to ensure it's on top
                return originalOrder;
            }

            // Try to get SpriteRenderer component (for world space cards)
            var spriteRenderer = cardView.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                int originalOrder = spriteRenderer.sortingOrder;
                spriteRenderer.sortingOrder = 1000;
                return originalOrder;
            }

            // Try to get child components
            var childCanvas = cardView.GetComponentInChildren<Canvas>();
            if (childCanvas != null)
            {
                int originalOrder = childCanvas.sortingOrder;
                childCanvas.sortingOrder = 1000;
                return originalOrder;
            }

            var childSpriteRenderer = cardView.GetComponentInChildren<SpriteRenderer>();
            if (childSpriteRenderer != null)
            {
                int originalOrder = childSpriteRenderer.sortingOrder;
                childSpriteRenderer.sortingOrder = 1000;
                return originalOrder;
            }

            return 0; // Default if no sorting component found
        }

        private void RestoreCardSortingOrder(CardView cardView, int originalSortingOrder)
        {
            // Restore Canvas sorting order
            var canvas = cardView.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = originalSortingOrder;
                return;
            }

            // Restore SpriteRenderer sorting order
            var spriteRenderer = cardView.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
                return;
            }

            // Restore child component sorting orders
            var childCanvas = cardView.GetComponentInChildren<Canvas>();
            if (childCanvas != null)
            {
                childCanvas.sortingOrder = originalSortingOrder;
                return;
            }

            var childSpriteRenderer = cardView.GetComponentInChildren<SpriteRenderer>();
            if (childSpriteRenderer != null)
            {
                childSpriteRenderer.sortingOrder = originalSortingOrder;
                return;
            }
        }
    }
}