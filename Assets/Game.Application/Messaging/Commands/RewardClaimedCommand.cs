using Game.Domain.Structs;

namespace Game.Application.Messaging.Commands
{
    public class RewardClaimedCommand : ICommand
    {
        public Reward Reward { get; }

        public RewardClaimedCommand(Reward reward)
        {
            Reward = reward;
        }
    }
}