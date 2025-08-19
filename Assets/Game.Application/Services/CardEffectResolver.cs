using System;
using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Application.Services
{
    public class CardEffectResolver : ICardEffectResolver
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IRandomService _rng;

        public CardEffectResolver(IEventBus bus, ILoggerService log, IRandomService rng)
        {
            _bus = bus;
            _log = log;
            _rng = rng;
        }

        public void ResolveCardEffects(AbilityCard card, MonsterEntity caster, MonsterEntity primaryTarget, 
            List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (caster == null)
                throw new ArgumentNullException(nameof(caster));

            _log?.Log($"{caster.MonsterName} plays card: {card.Name}");

            foreach (var effect in card.Effects)
            {
                var targets = GetValidTargetsForEffect(effect, caster, primaryTarget, allEnemies, allAllies);
                
                foreach (var target in targets)
                {
                    ResolveEffect(effect, caster, target);
                }
            }
        }

        public void ResolveEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity target)
        {
            if (target == null || target.IsDead)
                return;

            var beforeHP = target.CurrentHP;

            switch (effect.Type)
            {
                case EffectType.Damage:
                    target.TakeDamage(effect.Value);
                    var damageDealt = beforeHP - target.CurrentHP;
                    _bus.Publish(new DamageAppliedEvent(caster, target, damageDealt));
                    _log?.Log($"{caster.MonsterName} deals {damageDealt} damage to {target.MonsterName}");
                    break;

                case EffectType.Heal:
                    var healAmount = Math.Min(effect.Value, target.MaxHealth - target.CurrentHP);
                    target.Heal(healAmount);
                    _bus.Publish(new DamageAppliedEvent(caster, target, -healAmount)); // Negative damage for healing
                    _log?.Log($"{caster.MonsterName} heals {target.MonsterName} for {healAmount} HP");
                    break;

                case EffectType.Defend:
                    target.AddStatusEffect(new StatusEffect(effect.Type, effect.Value, effect.Duration, "Defend"));
                    _log?.Log($"{target.MonsterName} gains defense from {caster.MonsterName}'s card");
                    break;

                // Add other effect types as needed
                default:
                    _log?.LogWarning($"Unhandled effect type: {effect.Type}");
                    break;
            }

            if (target.IsDead)
            {
                _bus.Publish(new MonsterFaintedEvent(target));
            }
        }

        public List<MonsterEntity> GetValidTargetsForEffect(AbilityEffect effect, MonsterEntity caster, 
            MonsterEntity primaryTarget, List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies)
        {
            var targets = new List<MonsterEntity>();

            switch (effect.TargetType)
            {
                case TargetType.Self:
                    targets.Add(caster);
                    break;

                case TargetType.SingleEnemy:
                    if (primaryTarget != null && allEnemies.Contains(primaryTarget) && !primaryTarget.IsDead)
                        targets.Add(primaryTarget);
                    break;

                case TargetType.SingleAlly:
                    if (primaryTarget != null && allAllies.Contains(primaryTarget) && !primaryTarget.IsDead)
                        targets.Add(primaryTarget);
                    else if (primaryTarget == caster)
                        targets.Add(caster);
                    break;

                case TargetType.AllEnemies:
                    targets.AddRange(allEnemies.Where(e => !e.IsDead));
                    break;

                case TargetType.AllAllies:
                    targets.AddRange(allAllies.Where(a => !a.IsDead));
                    break;

                case TargetType.Random:
                    var allPossible = allEnemies.Where(e => !e.IsDead).ToList();
                    if (allPossible.Count > 0)
                    {
                        var randomIndex = _rng.Range(0, allPossible.Count);
                        targets.Add(allPossible[randomIndex]);
                    }
                    break;
            }

            return targets;
        }
    }
}