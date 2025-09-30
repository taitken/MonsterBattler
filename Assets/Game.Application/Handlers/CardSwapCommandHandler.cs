using Game.Application.Messaging.Commands;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.Player;
using Game.Application.Repositories;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using System.Linq;
using UnityEngine;

namespace Game.Application.Handlers
{
    public class CardSwapCommandHandler : ICommandHandler<CardSwapCommand>
    {
        private readonly IPlayerDataRepository _playerDataRepository;
        private readonly IEventBus _eventBus;

        public CardSwapCommandHandler(IPlayerDataRepository playerDataRepository, IEventBus eventBus)
        {
            _playerDataRepository = playerDataRepository;
            _eventBus = eventBus;
        }

        public void Handle(CardSwapCommand command)
        {
            Debug.Log($"Handling CardSwapCommand: {command.Card1Id} ({command.Card1Location}) <-> {command.Card2Id} ({command.Card2Location})");

            // Get game data
            var playerTeam = _playerDataRepository.GetPlayerTeam();
            var backpack = _playerDataRepository.GetBackpack();

            // Find the cards
            var (card1, card1Monster) = FindCard(command.Card1Id, command.Card1Location, playerTeam, backpack);
            var (card2, card2Monster) = FindCard(command.Card2Id, command.Card2Location, playerTeam, backpack);

            if (card1 == null || card2 == null)
            {
                Debug.LogError($"Could not find one or both cards for swap: Card1={card1 != null}, Card2={card2 != null}");
                return;
            }

            // Perform the swap based on locations
            PerformSwap(card1, command.Card1Location, card1Monster, card2, command.Card2Location, card2Monster, playerTeam, backpack);

            // Save changes
            _playerDataRepository.UpdatePlayerTeam(playerTeam);
            _playerDataRepository.UpdateBackpack(backpack);

            // Publish event to notify UI of changes
            _eventBus.Publish(new CardsUpdatedEvent());

            Debug.Log("Card swap completed successfully");
        }

        private (AbilityCard card, Domain.Entities.MonsterEntity monster) FindCard(System.Guid cardId, CardOriginType location,
            System.Collections.Generic.List<Domain.Entities.MonsterEntity> playerTeam, Domain.Entities.Player.BackpackEntity backpack)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    foreach (var monster in playerTeam)
                    {
                        if (monster.AbilityDeck != null)
                        {
                            var card = monster.AbilityDeck.AllCards.FirstOrDefault(c => c.Id == cardId);
                            if (card != null)
                            {
                                return (card, monster);
                            }
                        }
                    }
                    break;

                case CardOriginType.Backpack:
                    // Cards array may contain nulls for empty slots, so filter them out first
                    var backpackCard = backpack.Cards.Where(c => c != null).FirstOrDefault(c => c.Id == cardId);
                    if (backpackCard != null)
                    {
                        return (backpackCard, null);
                    }
                    break;
            }

            return (null, null);
        }

        private void PerformSwap(AbilityCard card1, CardOriginType card1Location, Domain.Entities.MonsterEntity card1Monster,
                                AbilityCard card2, CardOriginType card2Location, Domain.Entities.MonsterEntity card2Monster,
                                System.Collections.Generic.List<Domain.Entities.MonsterEntity> playerTeam,
                                Domain.Entities.Player.BackpackEntity backpack)
        {
            // If both cards are in the same location, use indexed swap to preserve ordering
            if (card1Location == card2Location)
            {
                if (card1Location == CardOriginType.Backpack)
                {
                    int index1 = backpack.GetCardIndex(card1);
                    int index2 = backpack.GetCardIndex(card2);

                    if (index1 >= 0 && index2 >= 0)
                    {
                        backpack.TrySwapCards(index1, index2);
                        Debug.Log($"Swapped cards in backpack at indices {index1} and {index2}");
                        return;
                    }
                }
                else if (card1Location == CardOriginType.MonsterDeck && card1Monster == card2Monster)
                {
                    // Both cards are in the same monster's deck
                    if (card1Monster?.AbilityDeck != null)
                    {
                        int index1 = card1Monster.AbilityDeck.GetCardIndex(card1);
                        int index2 = card1Monster.AbilityDeck.GetCardIndex(card2);

                        if (index1 >= 0 && index2 >= 0)
                        {
                            card1Monster.AbilityDeck.TrySwapCards(index1, index2);
                            Debug.Log($"Swapped cards in monster {card1Monster.MonsterName}'s deck at indices {index1} and {index2}");
                            return;
                        }
                    }
                }
            }

            // Cross-location swap: Get indices before removal, then swap at specific positions
            int card1Index = GetCardIndexInLocation(card1, card1Location, card1Monster, backpack);
            int card2Index = GetCardIndexInLocation(card2, card2Location, card2Monster, backpack);

            RemoveCardFromLocation(card1, card1Location, card1Monster, backpack);
            RemoveCardFromLocation(card2, card2Location, card2Monster, backpack);

            // Add cards to their new locations at the same slot positions (swapped)
            AddCardToLocationAtIndex(card1, card2Location, card2Index, card2Monster, backpack);
            AddCardToLocationAtIndex(card2, card1Location, card1Index, card1Monster, backpack);
        }

        private void RemoveCardFromLocation(AbilityCard card, CardOriginType location, Domain.Entities.MonsterEntity monster,
                                          Domain.Entities.Player.BackpackEntity backpack)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    if (monster?.AbilityDeck != null)
                    {
                        monster.AbilityDeck.RemoveCard(card);
                        Debug.Log($"Removed card {card.Name} from monster {monster.MonsterName}'s deck");
                    }
                    break;

                case CardOriginType.Backpack:
                    backpack.TryRemoveCard(card);
                    Debug.Log($"Removed card {card.Name} from backpack");
                    break;
            }
        }

        private void AddCardToLocation(AbilityCard card, CardOriginType location, Domain.Entities.MonsterEntity monster,
                                     Domain.Entities.Player.BackpackEntity backpack)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    if (monster?.AbilityDeck != null)
                    {
                        monster.AbilityDeck.AddCard(card);
                        Debug.Log($"Added card {card.Name} to monster {monster.MonsterName}'s deck");
                    }
                    break;

                case CardOriginType.Backpack:
                    if (!backpack.TryAddCard(card))
                    {
                        Debug.LogWarning($"Could not add card {card.Name} to backpack - backpack may be full");
                    }
                    else
                    {
                        Debug.Log($"Added card {card.Name} to backpack");
                    }
                    break;
            }
        }

        private int GetCardIndexInLocation(AbilityCard card, CardOriginType location, Domain.Entities.MonsterEntity monster,
                                          Domain.Entities.Player.BackpackEntity backpack)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    return monster?.AbilityDeck?.GetCardIndex(card) ?? -1;

                case CardOriginType.Backpack:
                    return backpack.GetCardIndex(card);

                default:
                    return -1;
            }
        }

        private void AddCardToLocationAtIndex(AbilityCard card, CardOriginType location, int index,
                                             Domain.Entities.MonsterEntity monster,
                                             Domain.Entities.Player.BackpackEntity backpack)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    if (monster?.AbilityDeck != null)
                    {
                        if (index >= 0 && !monster.AbilityDeck.TryInsertCardAt(index, card))
                        {
                            // Fallback: add to end
                            monster.AbilityDeck.AddCard(card);
                        }
                        Debug.Log($"Added card {card.Name} to monster {monster.MonsterName}'s deck at index {index}");
                    }
                    break;

                case CardOriginType.Backpack:
                    if (index >= 0 && !backpack.TrySetCardAt(index, card))
                    {
                        // Fallback: add to first available slot
                        backpack.TryAddCard(card);
                    }
                    Debug.Log($"Added card {card.Name} to backpack at slot {index}");
                    break;
            }
        }
    }
}