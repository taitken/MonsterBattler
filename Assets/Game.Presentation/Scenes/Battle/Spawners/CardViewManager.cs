using System.Collections;
using System.Collections.Generic;
using Game.Presentation.Shared.Views;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Domain.Entities;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Core.Interfaces;
using UnityEngine;

namespace Game.Presentation.Scenes.Battle.Spawners
{
    public class CardViewManager : MonoBehaviour
    {
        private ICardViewFactory _cardFactory;
        private Dictionary<MonsterEntity, CardView> _drawnCardViews = new();

        public void Initialize(ICardViewFactory cardFactory)
        {
            _cardFactory = cardFactory;
        }

        public List<CardView> CreateCardsForDraw(IReadOnlyList<CardsDrawnEvent.DrawnCard> drawnCards, IViewRegistryService viewRegistry)
        {
            var cardViews = new List<CardView>();

            // Clear any previous drawn card views
            ClearAllDrawnCards();

            // Create card views for all drawn cards
            foreach (var drawnCard in drawnCards)
            {
                // Find the monster's view to get deck icon position
                viewRegistry.TryGet(drawnCard.Monster.Id, out MonsterView monsterView);
                if (monsterView == null)
                {
                    Debug.LogWarning($"Could not find view for monster {drawnCard.Monster.MonsterName}");
                    continue;
                }

                // Get deck icon position
                var deckIconPosition = monsterView.DeckIconWorldPosition;
                
                // Spawn card at deck icon position 
                var cardView = _cardFactory.Create(drawnCard.Card, deckIconPosition);
                cardViews.Add(cardView);
                
                // Store the card view for later reuse
                _drawnCardViews[drawnCard.Monster] = cardView;
            }

            return cardViews;
        }

        public CardView GetDrawnCard(MonsterEntity monster)
        {
            return _drawnCardViews.TryGetValue(monster, out CardView cardView) ? cardView : null;
        }

        public CardView TakeDrawnCard(MonsterEntity monster)
        {
            if (_drawnCardViews.TryGetValue(monster, out CardView cardView))
            {
                _drawnCardViews.Remove(monster);
                return cardView;
            }
            return null;
        }

        public void ClearAllDrawnCards()
        {
            foreach (var cardView in _drawnCardViews.Values)
            {
                if (cardView != null)
                    Destroy(cardView.gameObject);
            }
            _drawnCardViews.Clear();
        }

        public void DestroyCardForMonster(MonsterEntity monster)
        {
            if (_drawnCardViews.TryGetValue(monster, out CardView cardView))
            {
                _drawnCardViews.Remove(monster);
                if (cardView != null)
                {
                    Debug.Log($"Fading out card for dead monster: {monster.MonsterName}");
                    StartCoroutine(FadeOutAndDestroyCard(cardView));
                }
            }
        }

        private IEnumerator FadeOutAndDestroyCard(CardView cardView)
        {
            if (cardView == null) yield break;

            // Get or add CanvasGroup for alpha control
            var canvasGroup = cardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();

            float fadeDuration = 0.5f;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            // Fade out the card
            while (elapsed < fadeDuration && cardView != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            // Destroy the card
            if (cardView != null)
            {
                Debug.Log($"Card fade complete, destroying GameObject");
                Destroy(cardView.gameObject);
            }
        }

        public CardView CreateCardForAction(MonsterEntity caster, Domain.Entities.Abilities.AbilityCard card, IViewRegistryService viewRegistry)
        {
            // Fallback method for creating cards when pre-drawn card is not available
            viewRegistry.TryGet(caster.Id, out MonsterView casterView);
            if (casterView == null)
            {
                Debug.LogWarning($"Could not find view for caster {caster.MonsterName}");
                return null;
            }
            
            var finalCardPosition = casterView.transform.position + new Vector3(0, 3f, 0);
            return _cardFactory.Create(card, finalCardPosition);
        }

        public int GetDrawnCardCount()
        {
            return _drawnCardViews.Count;
        }

        public void OnDestroy()
        {
            ClearAllDrawnCards();
        }
    }
}