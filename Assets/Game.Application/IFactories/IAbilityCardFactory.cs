using System.Collections.Generic;
using Game.Domain.Entities.Abilities;

namespace Game.Application.IFactories
{
    public interface IAbilityCardFactory
    {
        AbilityCard CreateCard(string cardName);
        AbilityCard CreateCardById(string cardId);
        List<AbilityCard> CreateAllCards();
        Deck CreateStarterDeck();
        Deck CreateDeckFromCardNames(IEnumerable<string> cardNames);
    }
}