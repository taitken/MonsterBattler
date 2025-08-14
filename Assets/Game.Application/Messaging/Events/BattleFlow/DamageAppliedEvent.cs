using Game.Domain.Entities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct DamageAppliedEvent : IDomainEvent
    {
        public DamageAppliedEvent(MonsterEntity attacker, MonsterEntity target, int amount)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
        }
        public MonsterEntity Attacker { get; }
        public MonsterEntity Target { get; }
        public int Amount { get; }
    }
}