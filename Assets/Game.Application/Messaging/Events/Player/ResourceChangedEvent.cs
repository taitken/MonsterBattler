using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.Player
{
    public readonly struct ResourceChangedEvent : IDomainEvent
    {
        public ResourceChangedEvent(ResourceType resourceType, int newAmount, int previousAmount)
        {
            ResourceType = resourceType;
            NewAmount = newAmount;
            PreviousAmount = previousAmount;
        }

        public ResourceType ResourceType { get; }
        public int NewAmount { get; }
        public int PreviousAmount { get; }
    }
}