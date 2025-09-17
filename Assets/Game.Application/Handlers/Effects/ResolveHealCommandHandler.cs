using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using System.Linq;
using System;

namespace Game.Application.Handlers.Effects
{
    public class ResolveHealCommandHandler : ICommandHandler<ResolveHealCommand>
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public ResolveHealCommandHandler(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void Handle(ResolveHealCommand command)
        {
            if (command.Target.IsDead)
                return;

            var beforeHP = command.Target.CurrentHP;
            var healAmount = Math.Min(command.Value, command.Target.MaxHealth - command.Target.CurrentHP);

            command.Target.Heal(healAmount);

            _bus.Publish(new DamageAppliedEvent(command.Caster, command.Target, -healAmount, 0, command.WaitToken));
            _log?.Log($"{command.Caster.MonsterName} heals {command.Target.MonsterName} for {healAmount} HP");
        }
    }
}