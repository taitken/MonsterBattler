using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Entities.Abilities;

namespace Game.Domain.Entities.Player
{
    public class BackpackEntity : BaseEntity
    {
        private readonly AbilityCard[] _slots;
        private const int MaxCapacity = 5;

        public IReadOnlyList<AbilityCard> Cards => System.Array.AsReadOnly(_slots);
        public int Count => _slots.Count(c => c != null);
        public int RemainingCapacity => MaxCapacity - Count;
        public bool IsFull => Count >= MaxCapacity;
        public bool IsEmpty => Count == 0;

        public event Action<AbilityCard> OnCardAdded;
        public event Action<AbilityCard> OnCardRemoved;
        public event Action OnBackpackFull;

        public BackpackEntity()
        {
            _slots = new AbilityCard[MaxCapacity];
        }

        public BackpackEntity(IEnumerable<AbilityCard> initialCards)
        {
            _slots = new AbilityCard[MaxCapacity];

            if (initialCards != null)
            {
                int index = 0;
                foreach (var card in initialCards.Take(MaxCapacity))
                {
                    _slots[index++] = card;
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

            // Find first empty slot
            for (int i = 0; i < MaxCapacity; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = card;
                    OnCardAdded?.Invoke(card);
                    NotifyModelUpdated();
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveCard(AbilityCard card)
        {
            if (card == null)
                return false;

            for (int i = 0; i < MaxCapacity; i++)
            {
                if (_slots[i]?.Id == card.Id)
                {
                    _slots[i] = null;
                    OnCardRemoved?.Invoke(card);
                    NotifyModelUpdated();
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveCardAt(int index)
        {
            if (index < 0 || index >= MaxCapacity)
                return false;

            var card = _slots[index];
            if (card == null)
                return false;

            _slots[index] = null;
            OnCardRemoved?.Invoke(card);
            NotifyModelUpdated();
            return true;
        }

        public AbilityCard GetCardAt(int index)
        {
            if (index < 0 || index >= MaxCapacity)
                return null;

            return _slots[index];
        }

        public bool HasCard(AbilityCard card)
        {
            if (card == null)
                return false;

            for (int i = 0; i < MaxCapacity; i++)
            {
                if (_slots[i]?.Id == card.Id)
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            var cardsToRemove = _slots.Where(c => c != null).ToList();

            for (int i = 0; i < MaxCapacity; i++)
            {
                _slots[i] = null;
            }

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

        public bool TryInsertCardAt(int index, AbilityCard card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            if (index < 0 || index >= MaxCapacity)
                return false;

            // Check if slot is already occupied
            if (_slots[index] != null)
                return false;

            _slots[index] = card;
            OnCardAdded?.Invoke(card);
            NotifyModelUpdated();
            return true;
        }

        public bool TrySwapCards(int index1, int index2)
        {
            if (index1 < 0 || index1 >= MaxCapacity || index2 < 0 || index2 >= MaxCapacity)
                return false;

            (_slots[index1], _slots[index2]) = (_slots[index2], _slots[index1]);
            NotifyModelUpdated();
            return true;
        }

        public int GetCardIndex(AbilityCard card)
        {
            if (card == null)
                return -1;

            for (int i = 0; i < MaxCapacity; i++)
            {
                if (_slots[i]?.Id == card.Id)
                    return i;
            }

            return -1;
        }

        public bool TrySetCardAt(int index, AbilityCard card)
        {
            if (index < 0 || index >= MaxCapacity)
                return false;

            var oldCard = _slots[index];
            _slots[index] = card;

            if (oldCard != null)
                OnCardRemoved?.Invoke(oldCard);

            if (card != null)
                OnCardAdded?.Invoke(card);

            NotifyModelUpdated();
            return true;
        }

        public bool IsSlotEmpty(int index)
        {
            if (index < 0 || index >= MaxCapacity)
                return false;

            return _slots[index] == null;
        }
    }
}