using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using System.Linq;

namespace Game.Application.Handlers.Effects
{
    public class ResolveAmplifyCommandHandler : ICommandHandler<ResolveAmplifyCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IEffectPropertyService _effectProperties;

        public ResolveAmplifyCommandHandler(IEventBus bus, ILoggerService log, IEffectPropertyService effectProperties)
        {
            _bus = bus;
            _log = log;
            _effectProperties = effectProperties;
        }

        public void Handle(ResolveAmplifyCommand command)
        {
            if (command.Target.IsDead)
                return;

            var debuffEffects = command.Target.StatusEffects
                .Where(e => !e.IsExpired && _effectProperties.IsDebuff(e.Type))
                .ToList();

            var debuffCount = debuffEffects.Count;

            if (debuffCount == 0)
            {
                _log?.Log($"{command.Caster.MonsterName} amplifies {command.Target.MonsterName}, but they have no debuff effects");
                return;
            }

            foreach (var effect in debuffEffects)
            {
                effect.IncreaseStacks(command.Value);
            }

            _bus.Publish(new EffectAmplifiedEvent(command.Caster, command.Target, command.Value, debuffCount, command.WaitToken));
            _log?.Log($"{command.Caster.MonsterName} amplifies {debuffCount} debuff effects on {command.Target.MonsterName} by {command.Value} stacks each");
        }
    }
}