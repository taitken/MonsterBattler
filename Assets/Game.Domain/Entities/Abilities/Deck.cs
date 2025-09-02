using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain.Entities.Abilities
{
    public class Deck : BaseEntity
    {
        private readonly List<AbilityCard> _drawPile = new();
        private readonly List<AbilityCard> _discardPile = new();
        private readonly Random _random;

        public IReadOnlyList<AbilityCard> AllCards => _drawPile.Concat(_discardPile).ToList().AsReadOnly();
        public IReadOnlyList<AbilityCard> AvailableCards => _drawPile.AsReadOnly();
        public int CardsInDeck => _drawPile.Count;
        public int CardsInDiscard => _discardPile.Count;

        public event Action OnDeckShuffled;
        public event Action<AbilityCard> OnCardPlayed;
        public event Action<AbilityCard> OnCardDrawn;

        public Deck(IEnumerable<AbilityCard> cards, Random random = null)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            _drawPile.AddRange(cards);
            _random = random ?? new Random();
            Shuffle();
        }

        // Copy constructor for deep copying
        public Deck(Deck other, Random random = null)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _random = random ?? new Random();
            
            // Deep copy all cards from both draw and discard piles
            foreach (var card in other._drawPile)
            {
                _drawPile.Add(new AbilityCard(card));
            }
            
            foreach (var card in other._discardPile)
            {
                _discardPile.Add(new AbilityCard(card));
            }
            
            // Don't shuffle on copy to preserve the current state
        }

        public void PlayCard(AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (!_drawPile.Contains(card))
                throw new InvalidOperationException("Card not in deck");

            _drawPile.Remove(card);
            _discardPile.Add(card);
            OnCardPlayed?.Invoke(card);

            if (_drawPile.Count == 0 && _discardPile.Count > 0)
            {
                ReshuffleDiscardIntoDeck();
            }
            NotifyModelUpdated();
        }

        public bool IsCardAvailable(AbilityCard card) => _drawPile.Contains(card);

        public IEnumerable<AbilityCard> GetAvailableCards() => _drawPile.AsEnumerable();

        public AbilityCard DrawCard()
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count > 0)
                {
                    ReshuffleDiscardIntoDeck();
                }
                else
                {
                    throw new InvalidOperationException("No cards available to draw");
                }
            }

            var card = _drawPile[0];
            OnCardDrawn?.Invoke(card);
            NotifyModelUpdated();
            return card;
        }

        public AbilityCard DrawRandomCard()
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count > 0)
                {
                    ReshuffleDiscardIntoDeck();
                }
                else
                {
                    throw new InvalidOperationException("No cards available to draw");
                }
            }

            int randomIndex = _random.Next(_drawPile.Count);
            var card = _drawPile[randomIndex];
            OnCardDrawn?.Invoke(card);
            NotifyModelUpdated();
            return card;
        }

        public bool CanDrawCard() => _drawPile.Count > 0 || _discardPile.Count > 0;

        public void AddCard(AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            _drawPile.Add(card);
            NotifyModelUpdated();
        }

        public bool RemoveCard(AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            // Remove by ID to handle different instances of the same logical card
            var drawPileCard = _drawPile.FirstOrDefault(c => c.Id == card.Id);
            bool removed = false;
            
            if (drawPileCard != null)
            {
                removed = _drawPile.Remove(drawPileCard);
            }
            else
            {
                var discardPileCard = _discardPile.FirstOrDefault(c => c.Id == card.Id);
                if (discardPileCard != null)
                {
                    removed = _discardPile.Remove(discardPileCard);
                }
            }

            if (removed)
                NotifyModelUpdated();

            return removed;
        }

        private void ReshuffleDiscardIntoDeck()
        {
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle();
            OnDeckShuffled?.Invoke();
        }

        private void Shuffle()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
            }
        }
    }
}