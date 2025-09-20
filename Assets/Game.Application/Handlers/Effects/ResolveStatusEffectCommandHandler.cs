using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Services;
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

            var effectName = AbilityEffectDescriptionService.GetEffectTypeText(command.Type);
            var statusEffect = new StatusEffect(command.Type, command.Stacks, effectName);

            command.Target.AddStatusEffect(statusEffect);

            _effectProcessor.ProcessEffectApplied(command.Target, statusEffect);

            var effect = new AbilityEffect(command.Type, command.Stacks, TargetType.Self);
            _bus.Publish(new EffectAppliedEvent(command.Caster, command.Target, effect, command.Stacks, command.WaitToken));

            LogStatusEffectApplication(command);
        }

        private void LogStatusEffectApplication(ResolveStatusEffectCommand command)
        {
            var actionText = AbilityEffectDescriptionService.GetActionText(command.Type);
            var effectText = AbilityEffectDescriptionService.GetEffectTypeText(command.Type);

            _log?.Log($"{command.Caster.MonsterName} {actionText.ToLower()} {command.Stacks} {effectText} to {command.Target.MonsterName}");
        }
    }
}