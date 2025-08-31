using Game.Domain.Messaging;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.Rewards
{
    public class CardRewardSelectedEvent : IDomainEvent
    {
        public CardReward CardReward { get; }

        public CardRewardSelectedEvent(CardReward cardReward)
        {
            CardReward = cardReward;
        }
    }
}