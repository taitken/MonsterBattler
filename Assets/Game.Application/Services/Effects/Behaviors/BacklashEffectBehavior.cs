using Game.Application.DTOs.Effects;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class BacklashEffectBehavior : IOnDamageTakenBehavior
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public BacklashEffectBehavior(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public DamageModificationResult ModifyDamage(MonsterEntity target, int incomingDamage, MonsterEntity source, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Value <= 0 || incomingDamage <= 0 || source == target)
                return DamageModificationResult.NoModification(incomingDamage);

            // Deal backlash damage equal to stacks back to the attacker
            var backlashDamage = effect.Value;
            source.TakeDamage(backlashDamage);

            _log?.Log($"{target.MonsterName}'s Backlash ({effect.Value} stacks) deals {backlashDamage} damage to {source.MonsterName}");
            _bus.Publish(new DamageAppliedEvent(target, source, backlashDamage, 0, null));

            // Don't modify the original incoming damage
            return DamageModificationResult.NoModification(incomingDamage);
        }
    }
}