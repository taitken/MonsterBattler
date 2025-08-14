using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct BattleStartedEvent : IDomainEvent
    {
        public BattleStartedEvent(List<MonsterEntity> player, List<MonsterEntity> enemy)
        {
            Player = player;
            Enemy = enemy;
        }
        public List<MonsterEntity> Player { get; }
        public List<MonsterEntity> Enemy { get; }
    }
}