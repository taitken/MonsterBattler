using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using System.Linq;

namespace Game.Application.Handlers.Effects
{
    public class ResolveProliferateCommandHandler : ICommandHandler<ResolveProliferateCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IEffectPropertyService _effectProperties;

        public ResolveProliferateCommandHandler(IEventBus bus, ILoggerService log, IEffectPropertyService effectProperties)
        {
            _bus = bus;
            _log = log;
            _effectProperties = effectProperties;
        }

        public void Handle(ResolveProliferateCommand command)
        {
            if (command.Target.IsDead)
                return;

            var buffEffects = command.Target.StatusEffects
                .Where(e => !e.IsExpired && _effectProperties.IsBuff(e.Type))
                .ToList();

            var buffCount = buffEffects.Count;

            if (buffCount == 0)
            {
                _log?.Log($"{command.Caster.MonsterName} proliferates {command.Target.MonsterName}, but they have no buff effects");
                return;
            }

            foreach (var effect in buffEffects)
            {
                effect.IncreaseStacks(command.Value);
            }

            _bus.Publish(new EffectProliferatedEvent(command.Caster, command.Target, command.Value, buffCount, command.WaitToken));
            _log?.Log($"{command.Caster.MonsterName} proliferates {buffCount} buff effects on {command.Target.MonsterName} by {command.Value} stacks each");
        }
    }
}