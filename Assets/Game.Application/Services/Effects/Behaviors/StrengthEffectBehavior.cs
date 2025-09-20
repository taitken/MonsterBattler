using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class StrengthEffectBehavior : IOnDamageDealtBehavior
    {
        private readonly ILoggerService _log;

        public StrengthEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public int ModifyOutgoingDamage(MonsterEntity caster, MonsterEntity target, int baseDamage, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Stacks <= 0)
                return baseDamage;

            var bonusDamage = effect.Stacks;
            var finalDamage = baseDamage + bonusDamage;

            _log?.Log($"{caster.MonsterName}'s Strength ({effect.Stacks} stacks) increases damage by {bonusDamage} ({baseDamage} -> {finalDamage})");

            return finalDamage;
        }
    }
}