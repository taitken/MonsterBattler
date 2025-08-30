using System.Collections;
using System.Collections.Generic;
using Game.Application.DTOs;
using Game.Application.Enums;
using Game.Application.Interfaces;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Presentation.Shared.Views;
using Game.Presentation.Shared.Enums;
using Game.Presentation.Core.Interfaces;
using UnityEngine;
using Game.Application.Messaging;

namespace Game.Presentation.Scenes.Battle.Controllers
{
    public class CardAnimationController : MonoBehaviour
    {
        private const float ATTACK_ANIMATION_SPEED = 2.0f;
        private const float DEFEND_ANIMATION_SPEED = 2.0f;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;
        
        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
        }

        public IEnumerator AnimateCardsDrawn(IReadOnlyList<CardsDrawnEvent.DrawnCard> drawnCards, 
            IReadOnlyList<CardView> cardViews, BarrierToken completionToken)
        {
            var cardAnimationData = new List<(CardView cardView, Vector3 deckIconPosition, Vector3 finalPosition)>();

            // Calculate uniform Y position for all cards (based on first valid monster position)
            float? uniformCardY = null;
            
            // Phase 1: Calculate positions for all cards
            for (int i = 0; i < drawnCards.Count && i < cardViews.Count; i++)
            {
                var drawnCard = drawnCards[i];
                var cardView = cardViews[i];
                
                // Find the monster's view to position the card above them
                _viewRegistry.TryGet(drawnCard.Monster.Id, out MonsterView monsterView);
                if (monsterView == null)
                {
                    Debug.LogWarning($"Could not find view for monster {drawnCard.Monster.MonsterName}");
                    continue;
                }

                // Get deck icon position
                var deckIconPosition = monsterView.DeckIconWorldPosition;
                
                // Calculate final card position with uniform Y
                var baseCardPosition = monsterView.transform.position + new Vector3(0, 3.5f, 0);
                if (!uniformCardY.HasValue)
                {
                    uniformCardY = baseCardPosition.y; // Use first card's Y as reference
                }
                var finalCardPosition = new Vector3(baseCardPosition.x, uniformCardY.Value, baseCardPosition.z);
                
                // Store animation data
                cardAnimationData.Add((cardView, deckIconPosition, finalCardPosition));
            }

            // Phase 2: Animate all cards simultaneously with staggered start times
            var animationCoroutines = new List<Coroutine>();
            for (int i = 0; i < cardAnimationData.Count; i++)
            {
                var (cardView, deckIconPosition, finalPosition) = cardAnimationData[i];
                
                // Stagger the animation start time slightly for visual appeal
                float delay = i * 0.1f;
                animationCoroutines.Add(StartCoroutine(AnimateCardDrawReveal(cardView, deckIconPosition, finalPosition, delay)));
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

        public IEnumerator AnimateCardAction(CardView cardView, BarrierToken animationToken, CardAnimationType animationType, Vector3? targetPosition)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var startPosition = cardTransform.position;
            var floatTarget = startPosition + new Vector3(0, 0.5f, 0);
            
            // Bring card to front during animation
            var originalSortingOrder = BringCardToFront(cardView);

            // Phase 1: Float and potentially perform special animation
            if (animationType == CardAnimationType.Attack && targetPosition.HasValue)
            {
                yield return StartCoroutine(AnimateAttack(cardView, startPosition, floatTarget, targetPosition.Value, animationToken));
            }
            else if (animationType == CardAnimationType.Defend)
            {
                yield return StartCoroutine(AnimateDefend(cardView, startPosition, animationToken));
            }
            else
            {
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

            // Phase 2: Wait while damage/effects are resolved
            yield return new WaitForSeconds(1f);

            // Signal animation end
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.End));

            // Phase 3: Fade out and destroy the card
            if (cardView != null)
            {
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
        }

        private IEnumerator AnimateCardDrawReveal(CardView cardView, Vector3 deckIconPosition, Vector3 finalPosition, float delay)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            
            // Capture the original target scale BEFORE modifying it
            var targetScale = cardTransform.localScale;

            // Immediately set to starting state (small and stretched at deck icon)
            cardTransform.position = deckIconPosition;
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
                
                // Position: stretch from deck icon to final position
                cardTransform.position = Vector3.Lerp(deckIconPosition, finalPosition, easedT);
                
                // Scale: start stretched thin, end at target scale
                float scaleX = Mathf.Lerp(targetScale.x * 0.01f, targetScale.x, easedT);
                float scaleY = Mathf.Lerp(targetScale.y * 0.5f, targetScale.y, easedT);
                cardTransform.localScale = new Vector3(scaleX, scaleY, targetScale.z);
                
                yield return null;
            }
            
            // Ensure final position and scale
            cardTransform.position = finalPosition;
            cardTransform.localScale = targetScale;

            // Start gentle persistent floating motion - cards stay visible until played
            StartCoroutine(FloatCardPersistently(cardView, finalPosition));
        }

        private IEnumerator FloatCardPersistently(CardView cardView, Vector3 basePosition)
        {
            if (cardView == null) yield break;

            while (cardView != null)
            {
                float time = Time.time * 2f; // Slow floating speed
                float floatOffset = Mathf.Sin(time) * 0.1f; // Small gentle float
                cardView.transform.position = basePosition + new Vector3(0, floatOffset, 0);
                yield return null;
            }
        }

        private IEnumerator AnimateAttack(CardView cardView, Vector3 originalStartPosition, Vector3 floatTarget, Vector3 targetPosition, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var originalRotation = cardTransform.rotation;
            
            // Calculate direction to target and determine if attacking left or right
            Vector3 attackDirection = (targetPosition - originalStartPosition).normalized;
            bool attackingLeft = attackDirection.x < 0;
            
            // Phase 1: Wind up (0.3 seconds) - tilt back
            float windUpDuration = 0.3f / ATTACK_ANIMATION_SPEED;
            float elapsed = 0f;
            
            while (elapsed < windUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / windUpDuration;
                
                // Gentle float motion
                float floatOffset = Mathf.Sin(t * 3.14159f) * 0.1f;
                cardTransform.position = Vector3.Lerp(originalStartPosition, floatTarget, t * 0.3f) + new Vector3(0, floatOffset, 0);
                
                // Wind up rotation - tilt back away from target
                float windUpRotation = Mathf.Lerp(0f, attackingLeft ? 20f : -20f, Mathf.Sin(t * 3.14159f * 0.5f));
                cardTransform.rotation = originalRotation * Quaternion.Euler(0, 0, windUpRotation);
                
                yield return null;
            }
            
            // Phase 2: Strike forward (0.2 seconds) - quick rotation and slide toward enemy
            float strikeDuration = 0.2f / ATTACK_ANIMATION_SPEED;
            elapsed = 0f;
            Vector3 strikeStart = cardTransform.position;
            Vector3 strikeEnd = strikeStart + attackDirection * 0.5f; // Slide forward slightly
            
            while (elapsed < strikeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / strikeDuration;
                
                // Ease-out for snappy strike
                float easedT = 1f - Mathf.Pow(1f - t, 2f);
                
                // Strike position - slide toward target
                cardTransform.position = Vector3.Lerp(strikeStart, strikeEnd, easedT);
                
                // Strike rotation - swing toward target
                float startRotation = attackingLeft ? 20f : -20f;
                float endRotation = attackingLeft ? -25f : 25f;
                float strikeRotation = Mathf.Lerp(startRotation, endRotation, easedT);
                cardTransform.rotation = originalRotation * Quaternion.Euler(0, 0, strikeRotation);
                
                yield return null;
            }
            
            // Signal hit point - damage should be resolved now
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.Hit));
            
            // Phase 3: Return to position (0.4 seconds) - settle back to baseline position
            float returnDuration = 0.1f / ATTACK_ANIMATION_SPEED;
            elapsed = 0f;
            Vector3 returnStart = cardTransform.position;
            
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                
                // Ease-in for smooth return
                float easedT = Mathf.Pow(t, 0.5f);
                
                // Return position - dampen the bounce over time to settle at baseline (originalStartPosition)
                float bounceIntensity = Mathf.Lerp(0.2f, 0f, easedT); // Fade out bounce
                float floatOffset = Mathf.Sin(t * 3.14159f * 3) * bounceIntensity; // More bounces that fade out
                cardTransform.position = Vector3.Lerp(returnStart, originalStartPosition, easedT) + new Vector3(0, floatOffset, 0);
                
                // Return rotation - gradually return to original rotation
                cardTransform.rotation = Quaternion.Lerp(cardTransform.rotation, originalRotation, easedT);
                
                yield return null;
            }
            
            // Ensure final state - card should be at original position
            cardTransform.position = originalStartPosition;
            cardTransform.rotation = originalRotation;
        }

        private IEnumerator AnimateDefend(CardView cardView, Vector3 originalStartPosition, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var originalScale = cardTransform.localScale;
            
            // Phase 1: Snap to Attention (0.2 seconds) - Quick, rigid movement upward
            float snapDuration = 0.2f / DEFEND_ANIMATION_SPEED;
            float elapsed = 0f;
            Vector3 attentionPosition = originalStartPosition + new Vector3(0, 0.8f, 0);
            
            while (elapsed < snapDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / snapDuration;
                
                // Sharp ease-out for snappy movement
                float easedT = 1f - Mathf.Pow(1f - t, 4f);
                
                // Rigid upward movement
                cardTransform.position = Vector3.Lerp(originalStartPosition, attentionPosition, easedT);
                
                yield return null;
            }
            
            // Phase 2: Fortification Scale-Up (0.3 seconds) - Become larger and more imposing
            float fortifyDuration = 0.3f / DEFEND_ANIMATION_SPEED;
            elapsed = 0f;
            Vector3 fortifiedScale = originalScale * 1.2f; // 30% larger
            
            while (elapsed < fortifyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fortifyDuration;
                
                // Ease-out for strong defensive presence
                float easedT = 1f - Mathf.Pow(1f - t, 2f);
                
                // Scale up to show defensive strength
                cardTransform.localScale = Vector3.Lerp(originalScale, fortifiedScale, easedT);
                
                yield return null;
            }
            
            // Phase 3: Hold Fortified State (0.4 seconds) - Maintain defensive posture
            float holdDuration = 0.5f / DEFEND_ANIMATION_SPEED;
            elapsed = 0f;
            
            while (elapsed < holdDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / holdDuration;
                
                // Gentle pulsing to show active defense
                float pulseIntensity = 0.01f; // Small pulse as a percentage
                float currentPulse = Mathf.Sin(t * 8f) * pulseIntensity;
                cardTransform.localScale = fortifiedScale * (1f + currentPulse);
                
                yield return null;
            }
            
            // Phase 4: Return to Normal (0.1 seconds) - Release defensive stance
            float returnDuration = 0.1f / DEFEND_ANIMATION_SPEED;
            elapsed = 0f;
            Vector3 returnStartPos = cardTransform.position;
            
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                
                // Ease-in for smooth return
                float easedT = Mathf.Pow(t, 0.5f);
                
                // Return to original position and scale
                cardTransform.position = Vector3.Lerp(returnStartPos, originalStartPosition, easedT);
                cardTransform.localScale = Vector3.Lerp(fortifiedScale, originalScale, easedT);
                
                yield return null;
            }
            
            // Signal hit point - shield effect should activate now
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.Hit));
            
            // Ensure final state
            cardTransform.position = originalStartPosition;
            cardTransform.localScale = originalScale;
        }

        private int BringCardToFront(CardView cardView)
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