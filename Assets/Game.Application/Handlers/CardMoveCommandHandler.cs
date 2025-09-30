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
    public class CardMoveCommandHandler : ICommandHandler<CardMoveCommand>
    {
        private readonly IPlayerDataRepository _playerDataRepository;
        private readonly IEventBus _eventBus;

        public CardMoveCommandHandler(IPlayerDataRepository playerDataRepository, IEventBus eventBus)
        {
            _playerDataRepository = playerDataRepository;
            _eventBus = eventBus;
        }

        public void Handle(CardMoveCommand command)
        {
            Debug.Log($"Handling CardMoveCommand: {command.CardId} from {command.SourceLocation} to {command.TargetLocation}[{command.TargetIndex}]");

            // Get game data
            var playerTeam = _playerDataRepository.GetPlayerTeam();
            var backpack = _playerDataRepository.GetBackpack();

            // Find the card
            var (card, monster) = FindCard(command.CardId, command.SourceLocation, playerTeam, backpack);

            if (card == null)
            {
                Debug.LogError($"Could not find card {command.CardId} in {command.SourceLocation}");
                return;
            }

            // Remove from source
            RemoveCardFromLocation(card, command.SourceLocation, monster, backpack);

            // Insert at target index
            InsertCardAtLocation(card, command.TargetLocation, command.TargetIndex, monster, backpack, playerTeam);

            // Save changes
            _playerDataRepository.UpdatePlayerTeam(playerTeam);
            _playerDataRepository.UpdateBackpack(backpack);

            // Publish event to notify UI of changes
            _eventBus.Publish(new CardsUpdatedEvent());

            Debug.Log("Card move completed successfully");
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

        private void InsertCardAtLocation(AbilityCard card, CardOriginType location, int targetIndex,
                                        Domain.Entities.MonsterEntity sourceMonster,
                                        Domain.Entities.Player.BackpackEntity backpack,
                                        System.Collections.Generic.List<Domain.Entities.MonsterEntity> playerTeam)
        {
            switch (location)
            {
                case CardOriginType.MonsterDeck:
                    // For monster decks, use the same monster as source (intra-deck movement)
                    if (sourceMonster?.AbilityDeck != null)
                    {
                        if (!sourceMonster.AbilityDeck.TryInsertCardAt(targetIndex, card))
                        {
                            Debug.LogWarning($"Could not insert card {card.Name} at index {targetIndex} in monster deck");
                            // Fallback to adding at end
                            sourceMonster.AbilityDeck.AddCard(card);
                        }
                        else
                        {
                            Debug.Log($"Inserted card {card.Name} at index {targetIndex} in monster {sourceMonster.MonsterName}'s deck");
                        }
                    }
                    break;

                case CardOriginType.Backpack:
                    // Use TrySetCardAt to place card in specific slot (overwrites if occupied)
                    if (!backpack.TrySetCardAt(targetIndex, card))
                    {
                        Debug.LogWarning($"Could not set card {card.Name} at slot {targetIndex} in backpack");
                        // Fallback to adding at first available slot
                        backpack.TryAddCard(card);
                    }
                    else
                    {
                        Debug.Log($"Placed card {card.Name} at slot {targetIndex} in backpack");
                    }
                    break;
            }
        }
    }
}