using Game.Application.DTOs.Rewards;
using Game.Domain.Messaging;

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