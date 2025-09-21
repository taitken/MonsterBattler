using Game.Domain.Entities;
using Game.Application.DTOs;
using Game.Domain.Enums;

namespace Game.Application.Messaging
{
    public readonly struct ResolveDamageCommand : ICommand
    {
        public readonly MonsterEntity Caster;
        public readonly MonsterEntity Target;
        public readonly int Value;
        public readonly EffectType EffectType;
        public readonly BarrierToken? WaitToken;

        public ResolveDamageCommand(MonsterEntity caster, MonsterEntity target, int value, EffectType effectType, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Value = value;
            EffectType = effectType;
            WaitToken = waitToken;
        }
    }
}