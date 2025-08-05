using System;

public class MonsterEntity : BaseObjectModel
{
    public MonsterDefinition definition;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public event Action<int> OnHealthChanged;
    public event Action OnDied;

    public MonsterEntity(MonsterDefinition def)
    {
        definition = def;
        CurrentHealth = def.maxHealth;
        MaxHealth = def.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(amount);

        if (CurrentHealth <= 0)
            OnDied?.Invoke();
    }

    public bool IsDead => CurrentHealth <= 0;
}