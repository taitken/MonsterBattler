using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Domain.Entities.Abilities;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AbilityCardDatabase", menuName = "App/Databases/AbilityCardDatabase")]
    public class AbilityCardDatabase : ScriptableObject
    {
        [Header("Card Collection")]
        public List<AbilityCardData> cards = new();
        
        public IReadOnlyList<AbilityCardData> Cards => cards.AsReadOnly();
        
        public AbilityCardData GetCard(string cardName)
        {
            return cards.FirstOrDefault(card => card.cardName == cardName);
        }
        
        public AbilityCardData GetCardById(string cardId)
        {
            return cards.FirstOrDefault(card => card.name == cardId);
        }
        
        public List<AbilityCard> CreateAllCardEntities()
        {
            return cards.Select(cardData => cardData.ToEntity()).ToList();
        }
        
        public List<AbilityCard> CreateStarterDeck()
        {
            var starterCards = cards.Take(10).ToList();
            return starterCards.Select(cardData => cardData.ToEntity()).ToList();
        }
        
        void OnValidate()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && string.IsNullOrWhiteSpace(cards[i].cardName))
                {
                    Debug.LogWarning($"Card at index {i} has no name assigned", cards[i]);
                }
            }
        }
    }
}