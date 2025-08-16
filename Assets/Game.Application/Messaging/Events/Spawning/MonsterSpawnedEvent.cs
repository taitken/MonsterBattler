using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.Spawning
{
    public readonly struct MonsterSpawnedEvent : IDomainEvent
    {
        public MonsterSpawnedEvent(MonsterEntity monster, BattleTeam team, BarrierToken? spawnCompletionToken = null, int? expectedTotalCount = null)
        {
            Monster = monster;
            Team = team;
            SpawnCompletionToken = spawnCompletionToken;
            ExpectedTotalCount = expectedTotalCount;
        }
        public MonsterEntity Monster { get; }
        public BattleTeam Team { get; }
        public BarrierToken? SpawnCompletionToken { get; }
        public int? ExpectedTotalCount { get; }
    }
}