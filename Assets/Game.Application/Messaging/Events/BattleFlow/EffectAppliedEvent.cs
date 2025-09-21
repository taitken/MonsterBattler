using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Messaging;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct EffectAppliedEvent : IDomainEvent
    {
        public MonsterEntity Caster { get; }
        public MonsterEntity Target { get; }
        public StatusEffect Effect { get; }
        public int ActualValue { get; }
        public BarrierToken? WaitToken { get; }

        public EffectAppliedEvent(MonsterEntity caster, MonsterEntity target, StatusEffect effect, int actualValue, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
            ActualValue = actualValue;
            WaitToken = waitToken;
        }
    }
}