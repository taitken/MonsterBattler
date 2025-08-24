using System.Collections.Generic;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.IFactories
{
    public interface IAbilityCardFactory
    {
        AbilityCard CreateCard(string cardName);
        AbilityCard CreateCardById(string cardId);
        List<AbilityCard> CreateAllCards();
        Deck CreateStarterDeck();
        Deck CreateDeckFromCardNames(IEnumerable<string> cardNames);
        
        // Monster-specific starter deck methods
        List<AbilityCard> CreateStarterDeckForMonster(MonsterType monsterType);
        Deck CreateStarterDeckForMonsterEntity(MonsterType monsterType);
        Deck CreateStarterDeckForEnemy();
        bool HasStarterDeckForMonster(MonsterType monsterType);
    }
}