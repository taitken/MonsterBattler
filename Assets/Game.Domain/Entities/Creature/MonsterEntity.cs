using System;
using Game.Domain.Enums;

namespace Game.Domain.Entities
{
    public class MonsterEntity : BaseEntity
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackDamage { get; private set; }
        public MonsterType Type { get; private set; }
        public string MonsterName { get; private set; }
        public AttackDirection AttackDirection { get; private set; }
        public event Action<int> OnHealthChanged;
        public event Action OnAttack;
        public event Action OnDied;

        public MonsterEntity(
            int maxHealth,
            int attackDamage,
            MonsterType type,
            string monsterName,
            AttackDirection attackDirection
        )
        {
            CurrentHealth = maxHealth;
            MaxHealth = maxHealth;
            AttackDamage = attackDamage;
            Type = type;
            MonsterName = monsterName;
            AttackDirection = attackDirection;
        }

        public void TakeDamage(int amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(amount);

            if (CurrentHealth <= 0)
                OnDied?.Invoke();
        }

        public void Attack(MonsterEntity target)
        {
            OnAttack?.Invoke();
            target.TakeDamage(AttackDamage);
        }

        public bool IsDead => CurrentHealth <= 0;
    }
}