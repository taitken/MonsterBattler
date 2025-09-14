using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Entities.Abilities;

namespace Game.Domain.Entities.Player
{
    public class BackpackEntity : BaseEntity
    {
        private readonly List<AbilityCard> _cards;
        private const int MaxCapacity = 5;

        public IReadOnlyList<AbilityCard> Cards => _cards.AsReadOnly();
        public int Count => _cards.Count;
        public int RemainingCapacity => MaxCapacity - _cards.Count;
        public bool IsFull => _cards.Count >= MaxCapacity;
        public bool IsEmpty => _cards.Count == 0;

        public event Action<AbilityCard> OnCardAdded;
        public event Action<AbilityCard> OnCardRemoved;
        public event Action OnBackpackFull;

        public BackpackEntity()
        {
            _cards = new List<AbilityCard>();
        }

        public BackpackEntity(IEnumerable<AbilityCard> initialCards)
        {
            _cards = new List<AbilityCard>();
            
            if (initialCards != null)
            {
                foreach (var card in initialCards.Take(MaxCapacity))
                {
                    _cards.Add(card);
                }
            }
        }

        public bool TryAddCard(AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if (IsFull)
            {
                OnBackpackFull?.Invoke();
                return false;
            }

            _cards.Add(card);
            OnCardAdded?.Invoke(card);
            NotifyModelUpdated();
            return true;
        }

        public bool TryRemoveCard(AbilityCard card)
        {
            if (card == null)
                return false;

            var removed = _cards.Remove(card);
            if (removed)
            {
                OnCardRemoved?.Invoke(card);
                NotifyModelUpdated();
            }
            return removed;
        }

        public bool TryRemoveCardAt(int index)
        {
            if (index < 0 || index >= _cards.Count)
                return false;

            var card = _cards[index];
            _cards.RemoveAt(index);
            OnCardRemoved?.Invoke(card);
            NotifyModelUpdated();
            return true;
        }

        public AbilityCard GetCardAt(int index)
        {
            if (index < 0 || index >= _cards.Count)
                return null;

            return _cards[index];
        }

        public bool HasCard(AbilityCard card)
        {
            return card != null && _cards.Contains(card);
        }

        public void Clear()
        {
            var cardsToRemove = _cards.ToList();
            _cards.Clear();
            
            foreach (var card in cardsToRemove)
            {
                OnCardRemoved?.Invoke(card);
            }
            
            NotifyModelUpdated();
        }

        public bool CanAddCard()
        {
            return !IsFull;
        }
    }
}