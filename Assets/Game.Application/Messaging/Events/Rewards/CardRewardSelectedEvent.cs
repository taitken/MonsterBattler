using Game.Domain.Messaging;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.Rewards
{
    public class CardRewardSelectedEvent : IDomainEvent
    {
        public Reward Reward { get; }

        public CardRewardSelectedEvent(Reward reward)
        {
            Reward = reward;
        }
    }
}