using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct EffectProliferatedEvent : IDomainEvent
    {
        public MonsterEntity Caster { get; }
        public MonsterEntity Target { get; }
        public int StacksAdded { get; }
        public int EffectsCount { get; }
        public BarrierToken? WaitToken { get; }

        public EffectProliferatedEvent(MonsterEntity caster, MonsterEntity target, int stacksAdded, int effectsCount, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            StacksAdded = stacksAdded;
            EffectsCount = effectsCount;
            WaitToken = waitToken;
        }
    }
}