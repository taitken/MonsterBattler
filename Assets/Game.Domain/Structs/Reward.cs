using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct Reward
    {
        public ResourceType Type { get; }
        public int Amount { get; }
        public string DisplayText { get; }

        public Reward(ResourceType type, int amount, string displayText)
        {
            Type = type;
            Amount = amount;
            DisplayText = displayText;
        }
    }
}