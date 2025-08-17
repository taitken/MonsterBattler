using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain.Entities.Abilities
{
    public class Deck : BaseEntity
    {
        private readonly List<AbilityCard> _cards = new();
        private readonly List<AbilityCard> _discardPile = new();
        private readonly Random _random;
        
        public IReadOnlyList<AbilityCard> AvailableCards => _cards.AsReadOnly();
        public int CardsInDeck => _cards.Count;
        public int CardsInDiscard => _discardPile.Count;
        
        public event Action OnDeckShuffled;
        public event Action<AbilityCard> OnCardPlayed;
        
        public Deck(IEnumerable<AbilityCard> cards, Random random = null)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));
            
            _cards.AddRange(cards);
            _random = random ?? new Random();
            Shuffle();
        }
        
        public void PlayCard(AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (!_cards.Contains(card)) 
                throw new InvalidOperationException("Card not in deck");
            
            _cards.Remove(card);
            _discardPile.Add(card);
            OnCardPlayed?.Invoke(card);
            
            if (_cards.Count == 0 && _discardPile.Count > 0)
            {
                ReshuffleDiscardIntoDeck();
            }
        }
        
        public bool IsCardAvailable(AbilityCard card) => _cards.Contains(card);
        
        public IEnumerable<AbilityCard> GetAvailableCards() => _cards.AsEnumerable();
        
        private void ReshuffleDiscardIntoDeck()
        {
            _cards.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle();
            OnDeckShuffled?.Invoke();
        }
        
        private void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }
    }
}