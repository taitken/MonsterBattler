using Game.Application.DTOs.Effects;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.Services.Effects.Behaviors
{
    public class BacklashEffectBehavior : IAfterDamageTakenBehavior
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public BacklashEffectBehavior(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void OnAfterDamageTaken(MonsterEntity target, int damageTaken, MonsterEntity source, StatusEffect effect, EffectType damageTypeSource)
        {
            if (effect.IsExpired || effect.Stacks <= 0 || damageTaken <= 0 || source == target || damageTypeSource != EffectType.Damage)
                return;

            var backlashDamage = effect.Stacks;
            _bus.Publish(new ResolveDamageCommand(target, source, backlashDamage, EffectType.Backlash));

            _log?.Log($"{target.MonsterName}'s Backlash ({effect.Stacks} stacks) deals {backlashDamage} damage to {source.MonsterName}");
        }
    }
}