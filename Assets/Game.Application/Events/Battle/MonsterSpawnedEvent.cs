using Game.Core.Events;
using Game.Domain.Entities;

namespace Game.Application.Events.Battle
{
    public readonly struct MonsterSpawnedEvent : IEvent
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