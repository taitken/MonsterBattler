using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Interfaces.Effects;
using Game.Core.Logger;

namespace Game.Application.Handlers.Effects
{
    public class ResolveDamageCommandHandler : ICommandHandler<ResolveDamageCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IEffectProcessor _effectProcessor;

        public ResolveDamageCommandHandler(IEventBus bus, ILoggerService log, IEffectProcessor effectProcessor)
        {
            _bus = bus;
            _log = log;
            _effectProcessor = effectProcessor;
        }

        public void Handle(ResolveDamageCommand command)
        {
            if (command.Target.IsDead)
                return;

            var modifiedDamage = _effectProcessor.ProcessOutgoingDamage(command.Caster, command.Target, command.Value);

            var beforeHP = command.Target.CurrentHP;
            var amountBlocked = command.Target.TakeDamage(modifiedDamage);
            var damageDealt = beforeHP - command.Target.CurrentHP;

            _bus.Publish(new DamageAppliedEvent(command.Caster, command.Target, command.EffectType, damageDealt, amountBlocked, command.WaitToken));
            _log?.Log($"{command.Caster.MonsterName} deals {damageDealt} damage to {command.Target.MonsterName}");

            _effectProcessor.ProcessAfterDamageTaken(command.Target, damageDealt, command.Caster, command.EffectType);

            // Check if target died from the damage and publish fainted event
            if (command.Target.IsDead)
            {
                _bus.Publish(new MonsterFaintedEvent(command.Target));
            }
        }
    }
}