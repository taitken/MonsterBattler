using Game.Application.Messaging.Commands;
using Game.Application.Repositories;
using Game.Core.Logger;
using System.Linq;

namespace Game.Application.Handlers
{
    public class ReplaceCardCommandHandler : ICommandHandler<ReplaceCardCommand>
    {
        private readonly IPlayerDataRepository _playerDataRepository;

        public ReplaceCardCommandHandler(IPlayerDataRepository playerDataRepository)
        {
            _playerDataRepository = playerDataRepository;
        }

        public void Handle(ReplaceCardCommand command)
        {
            var playerTeam = _playerDataRepository.GetPlayerTeam();
            var targetMonster = playerTeam.FirstOrDefault(monster => monster.Id == command.MonsterId);
            
            if (targetMonster == null)
                return;

            // Remove the old card from the monster's deck
            targetMonster.AbilityDeck.RemoveCard(command.CardToRemove);
            
            // Add the removed card to the backpack (if there's space)
            var backpack = _playerDataRepository.GetBackpack();
            if (!backpack.TryAddCard(command.CardToRemove))
            {
                // Backpack is full - card is lost (could log a warning here)
                // Future enhancement: could show UI message about backpack being full
            }
            
            // Add the new card to the monster's deck
            targetMonster.AbilityDeck.AddCard(command.CardToAdd);
            
            // Save the updated player team and backpack
            _playerDataRepository.UpdatePlayerTeam(playerTeam);
            _playerDataRepository.UpdateBackpack(backpack);
        }
    }
}