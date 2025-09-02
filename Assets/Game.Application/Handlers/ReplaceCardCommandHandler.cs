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
            
            // Add the new card to the monster's deck
            targetMonster.AbilityDeck.AddCard(command.CardToAdd);
            
            // Save the updated player team
            _playerDataRepository.UpdatePlayerTeam(playerTeam);
        }
    }
}