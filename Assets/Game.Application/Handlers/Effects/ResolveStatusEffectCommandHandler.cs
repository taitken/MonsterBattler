using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
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

        public ResolveStatusEffectCommandHandler(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void Handle(ResolveStatusEffectCommand command)
        {
            if (command.Target.IsDead)
                return;

            var effectName = AbilityEffectDescriptionService.GetEffectTypeText(command.Type);
            var statusEffect = new StatusEffect(command.Type, command.Value, command.Duration, effectName);

            command.Target.AddStatusEffect(statusEffect);

            var effect = new AbilityEffect(command.Type, command.Value, TargetType.Self, command.Duration);
            _bus.Publish(new EffectAppliedEvent(command.Caster, command.Target, effect, command.Value, command.WaitToken));

            LogStatusEffectApplication(command);
        }

        private void LogStatusEffectApplication(ResolveStatusEffectCommand command)
        {
            var actionText = AbilityEffectDescriptionService.GetActionText(command.Type);
            var effectText = AbilityEffectDescriptionService.GetEffectTypeText(command.Type);
            var durationText = command.Duration > 0 ? $" for {command.Duration} turns" : "";

            _log?.Log($"{command.Caster.MonsterName} {actionText.ToLower()} {command.Value} {effectText} to {command.Target.MonsterName}{durationText}");
        }
    }
}