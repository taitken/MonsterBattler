using System.Collections.Generic;
using UnityEngine;
using Game.Application.IFactories;
using Game.Domain.Entities.Abilities;
using Game.Infrastructure.ScriptableObjects;

namespace Game.Infrastructure.Factories
{
    public class AbilityCardFactory : IAbilityCardFactory
    {
        private readonly AbilityCardDatabase _database;
        
        public AbilityCardFactory(AbilityCardDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }
        
        public AbilityCard CreateCard(string cardName)
        {
            var cardData = _database.GetCard(cardName);
            if (cardData == null)
            {
                Debug.LogError($"Card '{cardName}' not found in database");
                return null;
            }
            
            return cardData.ToEntity();
        }
        
        public AbilityCard CreateCardById(string cardId)
        {
            var cardData = _database.GetCardById(cardId);
            if (cardData == null)
            {
                Debug.LogError($"Card with ID '{cardId}' not found in database");
                return null;
            }
            
            return cardData.ToEntity();
        }
        
        public List<AbilityCard> CreateAllCards()
        {
            return _database.CreateAllCardEntities();
        }
        
        public Deck CreateStarterDeck()
        {
            var starterCards = _database.CreateStarterDeck();
            return new Deck(starterCards);
        }
        
        public Deck CreateDeckFromCardNames(IEnumerable<string> cardNames)
        {
            var cards = new List<AbilityCard>();
            foreach (var cardName in cardNames)
            {
                var card = CreateCard(cardName);
                if (card != null)
                {
                    cards.Add(card);
                }
            }
            
            return new Deck(cards);
        }
    }
}