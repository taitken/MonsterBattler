using System;
using Game.Domain.Enums;

namespace Game.Application.Messaging.Commands
{
    public class CardMoveCommand : ICommand
    {
        public Guid CardId { get; }
        public CardOriginType SourceLocation { get; }
        public CardOriginType TargetLocation { get; }
        public int TargetIndex { get; }

        public CardMoveCommand(Guid cardId, CardOriginType sourceLocation, CardOriginType targetLocation, int targetIndex)
        {
            CardId = cardId;
            SourceLocation = sourceLocation;
            TargetLocation = targetLocation;
            TargetIndex = targetIndex;
        }
    }
}