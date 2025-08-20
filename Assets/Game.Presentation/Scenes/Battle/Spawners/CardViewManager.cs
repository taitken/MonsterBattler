using System.Collections.Generic;
using Game.Presentation.Shared.Views;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Domain.Entities;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Core.Interfaces;
using UnityEngine;

namespace Game.Presentation.Scenes.Battle.Services
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

        public CardView CreateCardForAction(MonsterEntity caster, Game.Domain.Entities.Abilities.AbilityCard card, IViewRegistryService viewRegistry)
        {
            // Fallback method for creating cards when pre-drawn card is not available
            viewRegistry.TryGet(caster.Id, out MonsterView casterView);
            if (casterView == null)
            {
                Debug.LogWarning($"Could not find view for caster {caster.MonsterName}");
                return null;
            }
            
            var finalCardPosition = casterView.transform.position + new Vector3(0, 4f, 0);
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