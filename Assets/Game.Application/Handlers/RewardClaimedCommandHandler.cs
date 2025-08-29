using Game.Application.Messaging.Commands;
using Game.Application.Repositories;

namespace Game.Application.Handlers
{
    public class RewardClaimedCommandHandler : ICommandHandler<RewardClaimedCommand>
    {
        private readonly IPlayerDataRepository _playerDataRepository;

        public RewardClaimedCommandHandler(IPlayerDataRepository playerDataRepository)
        {
            _playerDataRepository = playerDataRepository;
        }

        public void Handle(RewardClaimedCommand command)
        {
            var playerResources = _playerDataRepository.GetPlayerResources();
            playerResources.AddResource(command.Reward.Type, command.Reward.Amount);
        }
    }
}