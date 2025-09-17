using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Application.DTOs;

namespace Game.Application.Messaging
{
    public readonly struct ResolveStatusEffectCommand : ICommand
    {
        public readonly MonsterEntity Caster;
        public readonly MonsterEntity Target;
        public readonly EffectType Type;
        public readonly int Value;
        public readonly int Duration;
        public readonly BarrierToken? WaitToken;

        public ResolveStatusEffectCommand(MonsterEntity caster, MonsterEntity target, EffectType type, int value, int duration, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Type = type;
            Value = value;
            Duration = duration;
            WaitToken = waitToken;
        }
    }
}