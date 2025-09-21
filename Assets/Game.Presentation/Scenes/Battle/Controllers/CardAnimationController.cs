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
using Game.Presentation.Scenes.Battle.Animations;

namespace Game.Presentation.Scenes.Battle.Controllers
{
    public class CardAnimationController : MonoBehaviour
    {
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;

        // Dedicated animators for each animation type
        private CardDrawAnimator _drawAnimator;
        private CardAttackAnimator _attackAnimator;
        private CardDefendAnimator _defendAnimator;
        private CardGeneralAnimator _generalAnimator;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;

            // Initialize all specialized animators
            InitializeAnimators();
        }

        private void InitializeAnimators()
        {
            // Create or get dedicated animator components
            _drawAnimator = GetOrAddComponent<CardDrawAnimator>();
            _attackAnimator = GetOrAddComponent<CardAttackAnimator>();
            _defendAnimator = GetOrAddComponent<CardDefendAnimator>();
            _generalAnimator = GetOrAddComponent<CardGeneralAnimator>();

            // Initialize each animator
            _drawAnimator.Initialize(_waitBarrier, _viewRegistry);
            _attackAnimator.Initialize(_waitBarrier, _viewRegistry);
            _defendAnimator.Initialize(_waitBarrier, _viewRegistry);
            _generalAnimator.Initialize(_waitBarrier, _viewRegistry);
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();
            return component;
        }

        public IEnumerator AnimateCardsDrawn(
            IReadOnlyList<CardView> cardViews, Dictionary<CardView, Vector2> deckIconPositions, BarrierToken completionToken)
        {
            // Delegate to the specialized draw animator
            yield return _drawAnimator.AnimateCardsDrawn(cardViews, deckIconPositions, completionToken);
        }

        public IEnumerator AnimateCardAction(CardView cardView, BarrierToken animationToken, CardAnimationType animationType, Vector3? targetPosition)
        {
            if (cardView == null) yield break;

            var cardTransform = cardView.transform;
            var startPosition = cardTransform.position;
            var floatTarget = startPosition + new Vector3(0, .5f, 0);

            // Bring card to front during animation
            var originalSortingOrder = _generalAnimator.BringCardToFront(cardView);

            // Phase 1: Delegate to specialized animators based on animation type
            if (animationType == CardAnimationType.Attack && targetPosition.HasValue)
            {
                yield return _attackAnimator.AnimateAttack(cardView, startPosition, floatTarget, targetPosition.Value, animationToken);
            }
            else if (animationType == CardAnimationType.Defend)
            {
                yield return _defendAnimator.AnimateDefend(cardView, startPosition, animationToken);
            }
            else
            {
                yield return _generalAnimator.AnimateGeneralAction(cardView, startPosition, floatTarget, animationToken);
            }

            // Phase 2: Wait while damage/effects are resolved
            yield return new WaitForSeconds(1f);

            // Signal animation end
            _waitBarrier.Signal(new BarrierKey(animationToken, (int)AttackPhase.End));

            // Phase 3: Fade out and destroy the card
            yield return _generalAnimator.FadeOutAndDestroy(cardView, originalSortingOrder);
        }

    }
}