using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct BattleEndedEvent : IDomainEvent
    {
        public BattleEndedEvent(BattleOutcome outcome, List<MonsterEntity> survivors, int turns)
        {
            Outcome = outcome;
            Survivors = survivors;
            Turns = turns;
        }
        public BattleOutcome Outcome { get; }
        public List<MonsterEntity> Survivors { get; }
        public int Turns { get; }
    }
}