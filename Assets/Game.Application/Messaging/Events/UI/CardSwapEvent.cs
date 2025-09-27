using System;
using Game.Application.Messaging;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.UI
{
    public class CardSwapEvent : IDomainEvent
    {
        public Guid Card1ModelId { get; }
        public CardOriginType Card1Origin { get; }
        public Guid Card2ModelId { get; }
        public CardOriginType Card2Origin { get; }
        public DateTime Timestamp { get; }

        public CardSwapEvent(Guid card1ModelId, Guid card2ModelId, CardOriginType card1Origin, CardOriginType card2Origin)
        {
            Card1ModelId = card1ModelId;
            Card2ModelId = card2ModelId;
            Card1Origin = card1Origin;
            Card2Origin = card2Origin;
            Timestamp = DateTime.UtcNow;
        }
    }
}