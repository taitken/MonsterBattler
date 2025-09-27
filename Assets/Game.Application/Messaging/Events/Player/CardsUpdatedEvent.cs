using System;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.Player
{
    public class CardsUpdatedEvent : IDomainEvent
    {
        public DateTime Timestamp { get; }

        public CardsUpdatedEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}