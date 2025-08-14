using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct TurnStartedEvent : IDomainEvent
    {
        public TurnStartedEvent(BattleTeam team)
        {
            Team = team;
        }
        public BattleTeam Team { get; }
    }
}