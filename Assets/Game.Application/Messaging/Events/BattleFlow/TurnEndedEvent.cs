using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct TurnEndedEvent : IDomainEvent
    {
        public TurnEndedEvent(BattleTeam team)
        {
            Team = team;
        }
        public BattleTeam Team { get; }
    }
}