using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct DamageAppliedEvent : IDomainEvent
    {
        public DamageAppliedEvent(MonsterEntity attacker, MonsterEntity target, EffectType damageSourceType, int amount, int amountBlocked = 0, BarrierToken? waitToken = null)
        {
            Attacker = attacker;
            Target = target;
            Amount = amount;
            DamageSourceType = damageSourceType;
            AmountBlocked = amountBlocked;
            WaitToken = waitToken;
        }
        public MonsterEntity Attacker { get; }
        public MonsterEntity Target { get; }
        public EffectType DamageSourceType { get; }
        public int Amount { get; }
        public int AmountBlocked { get; }
        public BarrierToken? WaitToken { get; }
    }
}