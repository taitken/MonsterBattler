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
        public readonly int Stacks;
        public readonly BarrierToken? WaitToken;

        public ResolveStatusEffectCommand(MonsterEntity caster, MonsterEntity target, EffectType stacks, int value, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Type = stacks;
            Stacks = value;
            WaitToken = waitToken;
        }
    }
}