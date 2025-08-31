using Game.Domain.Enums;

namespace Game.Domain.Entities.Rewards
{
    public class ExperienceReward : Reward
    {
        public ExperienceReward(int amount) : base(ResourceType.Experience, amount, $"{amount} XP")
        {
        }

        public override void Accept(IRewardVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}