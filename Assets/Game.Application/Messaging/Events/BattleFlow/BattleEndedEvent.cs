using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct BattleEndedEvent : IDomainEvent
    {
        public BattleEndedEvent(BattleResult result) => Result = result;
        public BattleResult Result { get; }
    }
}