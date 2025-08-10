using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Application.Messaging.Events
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