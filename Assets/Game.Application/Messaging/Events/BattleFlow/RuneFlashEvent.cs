using Game.Domain.Enums;
using Game.Domain.Messaging;
using System.Collections.Generic;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct RuneFlashEvent : IDomainEvent
    {
        public IReadOnlyList<RuneType> FlashingRunes { get; }

        public RuneFlashEvent(IReadOnlyList<RuneType> flashingRunes)
        {
            FlashingRunes = flashingRunes;
        }
    }
}