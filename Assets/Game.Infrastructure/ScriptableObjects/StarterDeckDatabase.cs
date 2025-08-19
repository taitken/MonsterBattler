using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Domain.Enums;
using Game.Domain.Entities.Abilities;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Starter Deck", menuName = "Cards/Starter Deck Configuration")]
    public class StarterDeckConfiguration : ScriptableObject
    {
        [Header("Monster Type")]
        public MonsterType monsterType;
        
        [Header("Deck Composition")]
        public List<CardEntry> cardEntries = new();
        
        [Header("Deck Info")]
        [TextArea(2, 3)]
        public string description;
        
        public int TotalCardCount => cardEntries.Sum(entry => entry.quantity);
        
        public List<AbilityCard> CreateDeck()
        {
            var cards = new List<AbilityCard>();
            foreach (var cardEntry in cardEntries)
            {
                if (cardEntry.cardData != null)
                {
                    for (int i = 0; i < cardEntry.quantity; i++)
                    {
                        cards.Add(cardEntry.cardData.ToEntity());
                    }
                }
            }
            return cards;
        }
        
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(name) && monsterType != MonsterType.Goald)
            {
                name = $"{monsterType}_StarterDeck";
            }
        }
    }
    
    [System.Serializable]
    public struct CardEntry
    {
        public AbilityCardData cardData;
        [Range(1, 10)]
        public int quantity;
    }
}