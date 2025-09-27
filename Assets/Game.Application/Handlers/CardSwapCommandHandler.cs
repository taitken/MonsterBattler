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
                    var backpackCard = backpack.Cards.FirstOrDefault(c => c.Id == cardId);
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
            // Remove both cards from their current locations
            RemoveCardFromLocation(card1, card1Location, card1Monster, backpack);
            RemoveCardFromLocation(card2, card2Location, card2Monster, backpack);

            // Add cards to their new locations (swapped)
            AddCardToLocation(card1, card2Location, card2Monster, backpack);
            AddCardToLocation(card2, card1Location, card1Monster, backpack);
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
    }
}