using Game.Domain.Enums;

namespace Game.Domain.Entities.Rewards
{
    public abstract class Reward
    {
        public ResourceType Type { get; }
        public int Amount { get; }
        public string DisplayText { get; }

        protected Reward(ResourceType type, int amount, string displayText)
        {
            Type = type;
            Amount = amount;
            DisplayText = displayText;
        }

        public abstract void Accept(IRewardVisitor visitor);
    }
}