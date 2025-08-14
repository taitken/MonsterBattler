using Game.Domain.Entities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct MonsterFaintedEvent : IDomainEvent
    {
        public MonsterFaintedEvent(MonsterEntity monster)
        {
            Monster = monster;
        }
        public MonsterEntity Monster { get; }
    }
}