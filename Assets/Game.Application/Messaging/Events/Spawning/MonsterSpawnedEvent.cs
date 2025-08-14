using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.Spawning
{
    public readonly struct MonsterSpawnedEvent : IDomainEvent
    {
        public MonsterSpawnedEvent(MonsterEntity monster, BattleTeam team)
        {
            Monster = monster;
            Team = team;
        }
        public MonsterEntity Monster { get; }
        public BattleTeam Team { get; }
    }
}