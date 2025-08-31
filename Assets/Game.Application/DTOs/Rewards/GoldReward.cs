using Game.Domain.Enums;

namespace Game.Application.DTOs.Rewards
{
    public class GoldReward : Reward
    {
        public GoldReward(int amount) : base(ResourceType.Gold, amount, $"{amount} Gold")
        {
        }

        public override void Accept(IRewardVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}