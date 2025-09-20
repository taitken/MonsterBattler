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
            if (effect.IsExpired || effect.Stacks <= 0 || incomingDamage <= 0 || source == target)
                return DamageModificationResult.NoModification(incomingDamage);

            // Deal backlash damage equal to stacks back to the attacker via EffectProcessor
            var backlashDamage = effect.Stacks;
            _bus.Publish(new ResolveDamageCommand(target, source, backlashDamage));

            _log?.Log($"{target.MonsterName}'s Backlash ({effect.Stacks} stacks) deals {backlashDamage} damage to {source.MonsterName}");

            // Don't modify the original incoming damage
            return DamageModificationResult.NoModification(incomingDamage);
        }
    }
}