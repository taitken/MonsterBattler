using Game.Domain.Entities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct DamageAppliedEvent : IDomainEvent
    {
        public DamageAppliedEvent(MonsterEntity attacker, MonsterEntity target, int amount, int amountBlocked = 0)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
            AmountBlocked = amountBlocked;
        }
        public MonsterEntity Attacker { get; }
        public MonsterEntity Target { get; }
        public int Amount { get; }
        public int AmountBlocked { get; }
    }
}