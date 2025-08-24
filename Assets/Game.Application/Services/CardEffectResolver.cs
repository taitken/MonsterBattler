using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.DTOs;
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

        public async Task ResolveCardEffectsAsync(AbilityCard card, MonsterEntity caster, MonsterEntity primaryTarget, 
            List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies, IInteractionBarrier waitBarrier, CancellationToken ct = default)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (caster == null)
                throw new ArgumentNullException(nameof(caster));

            _log?.Log($"{caster.MonsterName} plays card: {card.Name}");

            for (int effectIndex = 0; effectIndex < card.Effects.Count; effectIndex++)
            {
                var effect = card.Effects[effectIndex];
                var targets = GetValidTargetsForEffect(effect, caster, primaryTarget, allEnemies, allAllies);
                
                // Create barrier token for this effect if there are multiple effects
                BarrierToken? effectToken = null;
                if (card.Effects.Count > 1)
                {
                    effectToken = BarrierToken.New();
                }
                
                // Resolve effect for all targets
                foreach (var target in targets)
                {
                    ResolveEffect(effect, caster, target, effectToken);
                }
                
                // Wait for presentation layer to signal completion before next effect
                if (effectToken.HasValue && effectIndex < card.Effects.Count - 1)
                {
                    await waitBarrier.WaitAsync(new BarrierKey(effectToken.Value), ct);
                }
            }
        }

        public void ResolveEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity target, BarrierToken? effectToken = null)
        {
            if (target == null || target.IsDead)
                return;

            var beforeHP = target.CurrentHP;

            switch (effect.Type)
            {
                case EffectType.Damage:
                    var amountBlocked = target.TakeDamage(effect.Value);
                    var damageDealt = beforeHP - target.CurrentHP;
                    _bus.Publish(new DamageAppliedEvent(caster, target, damageDealt, amountBlocked, effectToken));
                    _log?.Log($"{caster.MonsterName} deals {damageDealt} damage to {target.MonsterName}");
                    break;

                case EffectType.Heal:
                    var healAmount = Math.Min(effect.Value, target.MaxHealth - target.CurrentHP);
                    target.Heal(healAmount);
                    _bus.Publish(new DamageAppliedEvent(caster, target, -healAmount, 0, effectToken)); // Negative damage for healing
                    _log?.Log($"{caster.MonsterName} heals {target.MonsterName} for {healAmount} HP");
                    break;

                case EffectType.Defend:
                    target.AddStatusEffect(new StatusEffect(effect.Type, effect.Value, effect.Duration, "Defend"));
                    _bus.Publish(new EffectAppliedEvent(caster, target, effect, effect.Value, effectToken));
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