using System.Collections;
using System.Collections.Generic;
using Game.Application.DTOs;
using Game.Application.Messaging;
using Game.Presentation.Shared.Views;
using Game.Presentation.Core.Interfaces;
using UnityEngine;
using Game.Application.Interfaces;
using static Game.Application.Messaging.Events.BattleFlow.CardsDrawnEvent;

namespace Game.Presentation.Scenes.Battle.Animations
{
    public class CardDrawAnimator : MonoBehaviour
    {
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
        }

        public IEnumerator AnimateCardsDrawn(
            IReadOnlyList<CardView> cardViews, Dictionary<CardView, Vector2> deckIconPositions, BarrierToken completionToken)
        {
            var cardAnimationData = new List<(CardView cardView, Vector2 deckIconCanvasPos)>();

            // Phase 1: Collect deck icon positions for all cards
            for (int i = 0; i < cardViews.Count; i++)
            {
                var cardView = cardViews[i];

                // Get the pre-calculated deck icon position from CardViewManager
                if (deckIconPositions.TryGetValue(cardView, out Vector2 deckIconPos))
                {
                    cardAnimationData.Add((cardView, deckIconPos));
                }
                else
                {
                    Debug.LogWarning($"No deck icon position found for card {i}");
                }
            }

            // Phase 2: Animate all cards simultaneously with staggered start times
            var animationCoroutines = new List<Coroutine>();
            for (int i = 0; i < cardAnimationData.Count; i++)
            {
                var (cardView, deckIconCanvasPos) = cardAnimationData[i];

                // Stagger the animation start time slightly for visual appeal
                float delay = i * 0.1f;
                animationCoroutines.Add(StartCoroutine(AnimateCardDrawReveal(cardView, deckIconCanvasPos, delay)));
            }

            // Wait for all animations to complete
            foreach (var coroutine in animationCoroutines)
            {
                yield return coroutine;
            }

            var elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
            }

            // Signal completion
            _waitBarrier.Signal(new BarrierKey(completionToken));
        }

        private IEnumerator AnimateCardDrawReveal(CardView cardView, Vector2 deckIconCanvasPos, float delay)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var rectTransform = cardView.GetComponent<RectTransform>();

            // Capture the original target scale BEFORE modifying it
            var targetScale = cardTransform.localScale;

            // Get current positions for UI cards or calculate positions for world cards
            Vector2 startCanvasPos = Vector2.zero;
            Vector2 endCanvasPos = Vector2.zero;
            Vector3 endWorldPos = Vector3.zero;
            bool isUICard = rectTransform != null;

            if (isUICard)
            {
                // UI Card - get current canvas position as end position (CardViewManager already positioned it correctly)
                endCanvasPos = rectTransform.anchoredPosition;

                // Use the pre-calculated deck icon canvas position from CardViewManager
                startCanvasPos = deckIconCanvasPos;

                Debug.Log($"Card animation: deck icon canvas pos: {startCanvasPos}, final canvas pos: {endCanvasPos}");

                // Set initial canvas position to start position
                rectTransform.anchoredPosition = startCanvasPos;
            }
            else
            {
                // World space card fallback - convert canvas position back to world space if needed
                endWorldPos = cardTransform.position;
                // For world space cards, just use current position as both start and end (minimal animation)
                cardTransform.position = endWorldPos;
            }
            cardTransform.localScale = new Vector3(targetScale.x * 0.01f, targetScale.y * 0.5f, targetScale.z);

            // Wait for stagger delay AFTER setting initial state
            yield return new WaitForSeconds(delay);

            // Animate the reverse suction stretch (0.5 seconds)
            float stretchDuration = 0.5f;
            float elapsed = 0f;

            while (elapsed < stretchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stretchDuration;

                // Use ease-out curve for natural reverse suction effect
                float easedT = 1f - Mathf.Pow(1f - t, 3f);

                // Position: stretch from start to final position
                if (isUICard)
                {
                    // UI Card - interpolate canvas position
                    rectTransform.anchoredPosition = Vector2.Lerp(startCanvasPos, endCanvasPos, easedT);
                }
                else
                {
                    // World space card - minimal animation (just scale)
                    // Position stays the same since we don't have monster reference
                }

                // Scale: start stretched thin, end at target scale
                float scaleX = Mathf.Lerp(targetScale.x * 0.01f, targetScale.x, easedT);
                float scaleY = Mathf.Lerp(targetScale.y * 0.5f, targetScale.y, easedT);
                cardTransform.localScale = new Vector3(scaleX, scaleY, targetScale.z);

                yield return null;
            }

            // Ensure final position and scale
            if (isUICard)
            {
                rectTransform.anchoredPosition = endCanvasPos;
            }
            else
            {
                cardTransform.position = endWorldPos;
            }
            cardTransform.localScale = targetScale;

            // Start gentle persistent floating motion - cards stay visible until played
            StartCoroutine(FloatCardPersistently(cardView, isUICard ? endCanvasPos : endWorldPos));
        }

        private IEnumerator FloatCardPersistently(CardView cardView, object basePosition)
        {
            if (cardView == null) yield break;

            var rectTransform = cardView.GetComponent<RectTransform>();
            Vector2 canvasBasePos = basePosition is Vector2 ? (Vector2)basePosition : Vector2.zero;
            Vector3 worldBasePos = basePosition is Vector3 ? (Vector3)basePosition : Vector3.zero;

            while (cardView != null)
            {
                float time = Time.time * 2f; // Slow floating speed
                float floatOffset = Mathf.Sin(time) * 10f; // Small gentle float in UI pixels

                if (rectTransform != null)
                {
                    // UI Card floating
                    rectTransform.anchoredPosition = canvasBasePos + new Vector2(0, floatOffset);
                }
                else
                {
                    // World space card floating
                    cardView.transform.position = worldBasePos + new Vector3(0, floatOffset * 0.01f, 0);
                }
                yield return null;
            }
        }
    }
}