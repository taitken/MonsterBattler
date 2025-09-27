using System;
using Game.Domain.Enums;

namespace Game.Application.Messaging.Commands
{
    public class CardSwapCommand : ICommand
    {
        public Guid Card1Id { get; }
        public CardOriginType Card1Location { get; }
        public Guid Card2Id { get; }
        public CardOriginType Card2Location { get; }

        public CardSwapCommand(Guid card1Id, CardOriginType card1Location, Guid card2Id, CardOriginType card2Location)
        {
            Card1Id = card1Id;
            Card1Location = card1Location;
            Card2Id = card2Id;
            Card2Location = card2Location;
        }
    }
}