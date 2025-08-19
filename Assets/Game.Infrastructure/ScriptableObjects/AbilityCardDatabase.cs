using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AbilityCardDatabase", menuName = "Cards/Databases/AbilityCardDatabase")]
    public class AbilityCardDatabase : ScriptableObject
    {
        [Header("Card Collection")]
        public List<AbilityCardData> cards = new();
        
        [Header("Starter Deck Configurations")]
        public List<StarterDeckConfiguration> starterDecks = new();
        
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
            var starterCards = cards.Take(3).ToList();
            return starterCards.Select(cardData => cardData.ToEntity()).ToList();
        }
        
        public List<AbilityCard> CreateStarterDeckForMonster(MonsterType monsterType)
        {
            var starterDeckConfig = GetStarterDeckConfiguration(monsterType);
            if (starterDeckConfig != null)
            {
                Debug.Log($"Found starter deck configuration for {monsterType} with {starterDeckConfig.TotalCardCount} cards", starterDeckConfig);
                return starterDeckConfig.CreateDeck();
            }
            
            Debug.LogWarning($"No starter deck configuration found for {monsterType}. Available configurations: [{string.Join(", ", starterDecks.Select(d => d.monsterType))}]. Falling back to generic starter deck.", this);
            return CreateStarterDeck();
        }
        
        public StarterDeckConfiguration GetStarterDeckConfiguration(MonsterType monsterType)
        {
            return starterDecks.FirstOrDefault(deck => deck.monsterType == monsterType);
        }
        
        public bool HasStarterDeckForMonster(MonsterType monsterType)
        {
            var hasConfig = GetStarterDeckConfiguration(monsterType) != null;
            if (!hasConfig)
            {
                Debug.LogWarning($"No starter deck configuration exists for {monsterType}", this);
            }
            return hasConfig;
        }
        
        void OnValidate()
        {
            // Validate cards
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && string.IsNullOrWhiteSpace(cards[i].cardName))
                {
                    Debug.LogWarning($"Card at index {i} has no name assigned", cards[i]);
                }
            }
            
            // Validate starter deck configurations
            var duplicateMonsterTypes = starterDecks
                .GroupBy(deck => deck.monsterType)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
                
            foreach (var duplicate in duplicateMonsterTypes)
            {
                Debug.LogWarning($"Multiple starter deck configurations found for {duplicate}. Only the first will be used.", this);
            }
            
            // Log missing starter decks for all monster types
            var allMonsterTypes = System.Enum.GetValues(typeof(MonsterType)).Cast<MonsterType>();
            var configuredTypes = starterDecks.Select(deck => deck.monsterType).ToHashSet();
            var missingTypes = allMonsterTypes.Where(type => !configuredTypes.Contains(type));
            
            if (missingTypes.Any())
            {
                Debug.LogWarning($"Missing starter deck configurations for: {string.Join(", ", missingTypes)}", this);
            }
        }
    }
}