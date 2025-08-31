using System.Collections.Generic;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct CardReward
    {
        public Reward BaseReward { get; }
        public IReadOnlyList<AbilityCard> CardChoices { get; }
        public string RewardId { get; }

        public CardReward(int amount, string displayText, IReadOnlyList<AbilityCard> cardChoices, string rewardId)
        {
            BaseReward = new Reward(ResourceType.Card, amount, displayText);
            CardChoices = cardChoices;
            RewardId = rewardId;
        }

        public ResourceType Type => BaseReward.Type;
        public int Amount => BaseReward.Amount;
        public string DisplayText => BaseReward.DisplayText;

        // Implicit conversion to Reward for backward compatibility
        public static implicit operator Reward(CardReward cardReward)
        {
            return cardReward.BaseReward;
        }
    }
}