using Game.Domain.Entities;
using Game.Application.DTOs;

namespace Game.Application.Messaging
{
    public readonly struct ResolveDamageCommand : ICommand
    {
        public readonly MonsterEntity Caster;
        public readonly MonsterEntity Target;
        public readonly int Value;
        public readonly BarrierToken? WaitToken;

        public ResolveDamageCommand(MonsterEntity caster, MonsterEntity target, int value, BarrierToken? waitToken = null)
        {
            Caster = caster;
            Target = target;
            Value = value;
            WaitToken = waitToken;
        }
    }
}