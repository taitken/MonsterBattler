using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct HealingAppliedEvent : IDomainEvent
    {
        public HealingAppliedEvent(MonsterEntity healer, MonsterEntity target, int amount, BarrierToken? waitToken = null)
        {
            Healer = healer;
            Target = target;
            Amount = amount;
            WaitToken = waitToken;
        }

        public MonsterEntity Healer { get; }
        public MonsterEntity Target { get; }
        public int Amount { get; }
        public BarrierToken? WaitToken { get; }
    }
}