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
    public class CardDefendAnimator : MonoBehaviour
    {
        private const float TOTAL_ANIMATION_TIME = .7f;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
        }

        public IEnumerator AnimateDefend(CardView cardView, Vector3 originalStartPosition, BarrierToken animationToken)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var originalScale = cardTransform.localScale;

            // Phase 1: Snap to Attention (0.2 seconds) - Quick, rigid movement upward
            float snapDuration = TOTAL_ANIMATION_TIME * 0.2f;
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
            float fortifyDuration = TOTAL_ANIMATION_TIME * 0.4f;
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
            float holdDuration = TOTAL_ANIMATION_TIME * 0.3f;
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
            float returnDuration =TOTAL_ANIMATION_TIME * 0.1f;
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
    }
}