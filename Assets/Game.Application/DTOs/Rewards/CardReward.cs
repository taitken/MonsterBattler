using System.Collections.Generic;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.DTOs.Rewards
{
    public class CardReward : Reward
    {
        public IReadOnlyList<AbilityCard> CardChoices { get; }

        public CardReward(int amount, string displayText, IReadOnlyList<AbilityCard> cardChoices) 
            : base(ResourceType.Card, amount, displayText)
        {
            CardChoices = cardChoices;
        }

        public override void Accept(IRewardVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}