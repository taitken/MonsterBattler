using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Structs;

namespace Game.Application.Messaging
{
    public readonly struct ResolveAmplifyCommand : ICommand
    {
        public readonly MonsterEntity Caster;
        public readonly MonsterEntity Target;
        public readonly int Value;
        public readonly BarrierToken? WaitToken;

        public ResolveAmplifyCommand(MonsterEntity caster, MonsterEntity target, int value, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Value = value;
            WaitToken = waitToken;
        }
    }
}