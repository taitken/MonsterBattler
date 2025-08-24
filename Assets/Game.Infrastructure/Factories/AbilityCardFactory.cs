using System.Collections.Generic;
using UnityEngine;
using Game.Application.IFactories;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
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

        public List<AbilityCard> CreateStarterDeckForMonster(MonsterType monsterType)
        {
            Debug.Log($"Creating starter deck for monster type: {monsterType}");
            var cards = _database.CreateStarterDeckForMonster(monsterType);
            Debug.Log($"Created starter deck for {monsterType} with {cards.Count} cards");
            return cards;
        }

        public List<AbilityCard> CreateDeckForEnemy()
        {
            Debug.Log($"Creating starter deck for enemy");
            var cards = _database.CreateEnemyDeck();
            Debug.Log($"Created starter deck for enemy with {cards.Count} cards");
            return cards;
        }

        public Deck CreateStarterDeckForEnemy()
        {
            Debug.Log($"Creating starter deck entity for enemy monster");
            var starterCards = CreateDeckForEnemy();
            if (starterCards.Count == 0)
            {
                Debug.LogWarning($"Starter deck for enemy is empty! Monster will have no abilities.");
            }
            return new Deck(starterCards);
        }

        public bool HasStarterDeckForMonster(MonsterType monsterType)
        {
            return _database.HasStarterDeckForMonster(monsterType);
        }

        public Deck CreateStarterDeckForMonsterEntity(MonsterType monsterType)
        {
            Debug.Log($"Creating starter deck entity for monster type: {monsterType}");
            var starterCards = CreateStarterDeckForMonster(monsterType);
            if (starterCards.Count == 0)
            {
                Debug.LogWarning($"Starter deck for {monsterType} is empty! Monster will have no abilities.");
            }
            return new Deck(starterCards);
        }
    }
}