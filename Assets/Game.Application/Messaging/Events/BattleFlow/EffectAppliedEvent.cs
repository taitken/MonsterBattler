using Game.Domain.Entities;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct EffectAppliedEvent
    {
        public MonsterEntity Caster { get; }
        public MonsterEntity Target { get; }
        public AbilityEffect Effect { get; }
        public int ActualValue { get; }

        public EffectAppliedEvent(MonsterEntity caster, MonsterEntity target, AbilityEffect effect, int actualValue)
        {
            Caster = caster;
            Target = target;
            Effect = effect;
            ActualValue = actualValue;
        }
    }
}