using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Structs;
using System.Linq;

namespace Game.Application.Handlers.Effects
{
    public class ResolveStatusEffectCommandHandler : ICommandHandler<ResolveStatusEffectCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IEffectProcessor _effectProcessor;

        public ResolveStatusEffectCommandHandler(IEventBus bus, ILoggerService log, IEffectProcessor effectProcessor)
        {
            _bus = bus;
            _log = log;
            _effectProcessor = effectProcessor;
        }

        public void Handle(ResolveStatusEffectCommand command)
        {
            if (command.Target.IsDead)
                return;

            var statusEffect = new StatusEffect(command.Type, command.Stacks);

            command.Target.AddStatusEffect(statusEffect);

            _effectProcessor.ProcessEffectApplied(command.Target, statusEffect);

            _bus.Publish(new EffectAppliedEvent(command.Caster, command.Target, statusEffect, command.Stacks, command.WaitToken));

            LogStatusEffectApplication(command);
        }

        private void LogStatusEffectApplication(ResolveStatusEffectCommand command)
        {
            _log?.Log($"{command.Caster.MonsterName} {command.Stacks} to {command.Target.MonsterName}");
        }
    }
}