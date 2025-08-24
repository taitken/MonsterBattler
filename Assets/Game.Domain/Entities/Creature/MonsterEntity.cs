using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Enums;
using Game.Domain.Entities.Abilities;

namespace Game.Domain.Entities
{
    public class MonsterEntity : BaseEntity
    {
        public int CurrentHP { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackDamage { get; private set; }
        public MonsterType Type { get; private set; }
        public string MonsterName { get; private set; }
        public AttackDirection AttackDirection { get; private set; }
        public Deck AbilityDeck { get; private set; }
        public IReadOnlyList<StatusEffect> StatusEffects => _statusEffects.AsReadOnly();
        
        private readonly List<StatusEffect> _statusEffects = new();
        
        public event Action<int> OnHealthChanged;
        public event Action OnAttack;
        public event Action OnDied;
        public event Action<AbilityCard> OnAbilityUsed;
        public event Action<StatusEffect> OnStatusEffectAdded;
        public event Action<StatusEffect> OnStatusEffectRemoved;

        public MonsterEntity(
            int maxHealth,
            int attackDamage,
            MonsterType type,
            string monsterName,
            AttackDirection attackDirection,
            Deck abilityDeck = null
        )
        {
            CurrentHP = maxHealth;
            MaxHealth = maxHealth;
            AttackDamage = attackDamage;
            Type = type;
            MonsterName = monsterName;
            AttackDirection = attackDirection;
            AbilityDeck = abilityDeck;
        }

        public int TakeDamage(int amount)
        {
            // Apply defend shields first
            var remainingDamage = amount;
            var totalBlocked = 0;
            var defendEffects = _statusEffects.Where(e => e.Type == EffectType.Defend).ToList();
            
            foreach (var defendEffect in defendEffects)
            {
                if (remainingDamage <= 0) break;
                
                var blocked = Math.Min(defendEffect.Value, remainingDamage);
                remainingDamage -= blocked;
                totalBlocked += blocked;
                defendEffect.ReduceValue(blocked);
            }
            
            // Apply remaining damage to health
            CurrentHP = Math.Max(0, CurrentHP - remainingDamage);
            OnHealthChanged?.Invoke(remainingDamage);

            if (CurrentHP <= 0)
                OnDied?.Invoke();
                
            // Return the total amount that was blocked
            return totalBlocked;
        }

        public void Heal(int amount)
        {
            var oldHP = CurrentHP;
            CurrentHP = Math.Min(MaxHealth, CurrentHP + amount);
            var actualHealing = CurrentHP - oldHP;
            OnHealthChanged?.Invoke(-actualHealing); // Negative for healing
        }

        public int Attack(MonsterEntity target)
        {
            OnAttack?.Invoke();
            return target.TakeDamage(AttackDamage);
        }

        public bool IsDead => CurrentHP <= 0;

        public void SetCurrentHP(int hp)
        {
            CurrentHP = Math.Clamp(hp, 0, MaxHealth);
        }

        public void RestoreToFullHealth()
        {
            CurrentHP = MaxHealth;
        }
        
        public void UseAbility(AbilityCard card)
        {
            if (AbilityDeck == null)
                throw new InvalidOperationException("Monster has no ability deck");
            if (!AbilityDeck.IsCardAvailable(card))
                throw new InvalidOperationException("Card not available in deck");
            
            AbilityDeck.PlayCard(card);
            OnAbilityUsed?.Invoke(card);
        }
        
        public void AddStatusEffect(StatusEffect statusEffect)
        {
            if (statusEffect == null)
                throw new ArgumentNullException(nameof(statusEffect));
            
            _statusEffects.Add(statusEffect);
            statusEffect.OnExpired += () => RemoveStatusEffect(statusEffect);
            OnStatusEffectAdded?.Invoke(statusEffect);
        }
        
        public void RemoveStatusEffect(StatusEffect statusEffect)
        {
            if (_statusEffects.Remove(statusEffect))
            {
                OnStatusEffectRemoved?.Invoke(statusEffect);
            }
        }
        
        public void ProcessStatusEffects()
        {
            var expiredEffects = new List<StatusEffect>();
            
            foreach (var effect in _statusEffects)
            {
                effect.ProcessTurn();
                if (effect.IsExpired)
                {
                    expiredEffects.Add(effect);
                }
            }
            
            foreach (var expired in expiredEffects)
            {
                RemoveStatusEffect(expired);
            }
        }
        
        public bool HasStatusEffect(EffectType effectType) =>
            _statusEffects.Any(e => e.Type == effectType);
        
        public IEnumerable<StatusEffect> GetStatusEffectsOfType(EffectType effectType) =>
            _statusEffects.Where(e => e.Type == effectType);
    }
}