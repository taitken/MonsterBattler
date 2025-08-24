using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Domain.Entities.Abilities;
using Game.Infrastructure.DTOs;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Enemy Deck", menuName = "Cards/Enemy Deck Configuration")]
    public class EnemyDeckConfiguration : ScriptableObject
    {
        
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
    }
}