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
    public class CardAttackAnimator : MonoBehaviour
    {
        private const float TOTAL_ANIMATION_TIME = .7f;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
        }

        public IEnumerator AnimateAttack(CardView cardView, Vector3 originalStartPosition, Vector3 floatTarget, Vector3 targetPosition, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var originalRotation = cardTransform.rotation;

            // Calculate direction to target and determine if attacking left or right
            Vector3 attackDirection = (targetPosition - originalStartPosition).normalized;
            bool attackingLeft = attackDirection.x < 0;

            // Phase 1: Wind up (0.3 seconds) - tilt back
            float windUpDuration = TOTAL_ANIMATION_TIME * 0.66f;
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
            float strikeDuration =  TOTAL_ANIMATION_TIME * 0.20f;
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
            float returnDuration = TOTAL_ANIMATION_TIME * 0.14f;
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
    }
}