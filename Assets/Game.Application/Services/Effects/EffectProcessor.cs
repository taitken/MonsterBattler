using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Services.Effects.Behaviors;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.Services.Effects
{
    public class EffectProcessor : IEffectProcessor
    {
        private readonly Dictionary<EffectType, IEffectBehavior> _behaviors;
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public EffectProcessor(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
            _behaviors = new Dictionary<EffectType, IEffectBehavior>();
            RegisterBehaviors();
        }

        public int ProcessDamageTaken(MonsterEntity target, int damage, MonsterEntity source)
        {
            var modifiedDamage = damage;
            var totalBlocked = 0;
            var totalReduced = 0;

            _log?.Log($"Processing damage taken: {damage} damage to {target.MonsterName}");

            foreach (var effect in target.StatusEffects.Where(e => !e.IsExpired).ToList())
            {
                if (_behaviors.TryGetValue(effect.Type, out var behavior) &&
                    behavior is IOnDamageTakenBehavior damageBehavior)
                {
                    var result = damageBehavior.ModifyDamage(target, modifiedDamage, source, effect);

                    if (result.WasModified)
                    {
                        modifiedDamage = result.FinalDamage;
                        totalBlocked += result.DamageBlocked;
                        totalReduced += result.DamageReduced;

                        _log?.Log($"Effect {effect.Type} modified damage: {damage} -> {modifiedDamage}");
                        _bus.Publish(new DamageModifiedByEffectEvent(target, effect, result));
                    }
                }
            }

            return modifiedDamage;
        }

        public bool ProcessCardPlayed(MonsterEntity player, AbilityCard card)
        {
            _log?.Log($"Processing card play: {player.MonsterName} playing {card.Name}");

            foreach (var effect in player.StatusEffects.Where(e => !e.IsExpired))
            {
                if (_behaviors.TryGetValue(effect.Type, out var behavior) &&
                    behavior is IOnCardPlayedBehavior cardBehavior)
                {
                    var result = cardBehavior.OnCardPlayed(player, card, effect);

                    if (!result.IsAllowed)
                    {
                        _log?.Log($"Card play prevented by {effect.Type}: {result.PreventionReason}");
                        _bus.Publish(new CardPlayPreventedEvent(player, card, effect, result.PreventionReason));
                        return false;
                    }
                }
            }

            return true;
        }

        public void ProcessTurnEnd(MonsterEntity target)
        {
            _log?.Log($"Processing turn end effects for {target.MonsterName}");

            var effectsToProcess = target.StatusEffects.Where(e => !e.IsExpired).ToList();

            foreach (var effect in effectsToProcess)
            {
                if (_behaviors.TryGetValue(effect.Type, out var behavior) &&
                    behavior is IOnTurnEndBehavior turnBehavior)
                {
                    _log?.Log($"Processing turn end for effect: {effect.Type}");
                    turnBehavior.OnTurnEnd(target, effect);
                }
            }
        }

        public void ProcessEffectApplied(MonsterEntity target, StatusEffect newEffect)
        {
            _log?.Log($"Processing effect applied: {newEffect.Type} on {target.MonsterName}");

            foreach (var existingEffect in target.StatusEffects.Where(e => e != newEffect && !e.IsExpired))
            {
                if (_behaviors.TryGetValue(existingEffect.Type, out var behavior) &&
                    behavior is IOnEffectAppliedBehavior effectBehavior)
                {
                    effectBehavior.OnEffectApplied(target, newEffect, existingEffect);
                }
            }
        }

        public int ProcessOutgoingDamage(MonsterEntity caster, MonsterEntity target, int baseDamage)
        {
            var modifiedDamage = baseDamage;

            _log?.Log($"Processing outgoing damage: {baseDamage} from {caster.MonsterName} to {target.MonsterName}");

            foreach (var effect in caster.StatusEffects.Where(e => !e.IsExpired).ToList())
            {
                if (_behaviors.TryGetValue(effect.Type, out var behavior) &&
                    behavior is IOnDamageDealtBehavior damageBehavior)
                {
                    var newDamage = damageBehavior.ModifyOutgoingDamage(caster, target, modifiedDamage, effect);

                    if (newDamage != modifiedDamage)
                    {
                        _log?.Log($"Effect {effect.Type} modified outgoing damage: {modifiedDamage} -> {newDamage}");
                        modifiedDamage = newDamage;
                    }
                }
            }

            return modifiedDamage;
        }

        public void ProcessAfterDamageTaken(MonsterEntity target, int damageTaken, MonsterEntity source, EffectType damageTypeSource)
        {
            _log?.Log($"Processing after damage taken: {damageTaken} damage to {target.MonsterName}");

            foreach (var effect in target.StatusEffects.Where(e => !e.IsExpired).ToList())
            {
                if (_behaviors.TryGetValue(effect.Type, out var behavior) &&
                    behavior is IAfterDamageTakenBehavior afterDamageBehavior)
                {
                    _log?.Log($"Triggering after damage behavior for effect: {effect.Type}");
                    afterDamageBehavior.OnAfterDamageTaken(target, damageTaken, source, effect, damageTypeSource);
                }
            }
        }

        private void RegisterBehaviors()
        {
            // Register effect behaviors
            RegisterBehavior(EffectType.Burn, new BurnEffectBehavior(_bus, _log));
            RegisterBehavior(EffectType.Block, new BlockEffectBehavior(_log));
            RegisterBehavior(EffectType.Stun, new StunEffectBehavior(_log));
            RegisterBehavior(EffectType.Poison, new PoisonEffectBehavior(_bus, _log));
            RegisterBehavior(EffectType.Fortify, new FortifyEffectBehavior(_log));
            RegisterBehavior(EffectType.Regenerate, new RegenerateEffectBehavior(_bus, _log));
            RegisterBehavior(EffectType.Frazzled, new FrazzledEffectBehavior(_log));
            RegisterBehavior(EffectType.Luck, new LuckEffectBehavior(_log));
            RegisterBehavior(EffectType.Strength, new StrengthEffectBehavior(_log));
            RegisterBehavior(EffectType.Backlash, new BacklashEffectBehavior(_bus, _log));

            _log?.Log($"EffectProcessor initialized with {_behaviors.Count} effect behaviors");
        }

        public void RegisterBehavior(EffectType effectType, IEffectBehavior behavior)
        {
            _behaviors[effectType] = behavior;
            _log?.Log($"Registered behavior for effect type: {effectType}");
        }
    }
}