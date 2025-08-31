using System.Collections.Generic;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Domain.Entities.Rewards
{
    public class CardReward : Reward
    {
        public IReadOnlyList<AbilityCard> CardChoices { get; }
        public string RewardId { get; }

        public CardReward(int amount, string displayText, IReadOnlyList<AbilityCard> cardChoices, string rewardId) 
            : base(ResourceType.Card, amount, displayText)
        {
            CardChoices = cardChoices;
            RewardId = rewardId;
        }

        public override void Accept(IRewardVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}