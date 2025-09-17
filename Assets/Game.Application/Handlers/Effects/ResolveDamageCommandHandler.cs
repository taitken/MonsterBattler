using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using System.Linq;

namespace Game.Application.Handlers.Effects
{
    public class ResolveDamageCommandHandler : ICommandHandler<ResolveDamageCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public ResolveDamageCommandHandler(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void Handle(ResolveDamageCommand command)
        {
            if (command.Target.IsDead)
                return;

            var beforeHP = command.Target.CurrentHP;
            var amountBlocked = command.Target.TakeDamage(command.Value);
            var damageDealt = beforeHP - command.Target.CurrentHP;

            _bus.Publish(new DamageAppliedEvent(command.Caster, command.Target, damageDealt, amountBlocked, command.WaitToken));
            _log?.Log($"{command.Caster.MonsterName} deals {damageDealt} damage to {command.Target.MonsterName}");
        }
    }
}