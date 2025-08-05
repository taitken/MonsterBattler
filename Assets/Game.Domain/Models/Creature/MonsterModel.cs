using System;

public class MonsterModel : BaseObjectModel
{
    public MonsterDefinition definition;
    public int CurrentHealth { get; private set; }
    public event Action OnHealthChanged;
    public event Action OnDied;

    public MonsterModel(MonsterDefinition def)
    {
        definition = def;
        CurrentHealth = def.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0)
            OnDied?.Invoke();
    }

    public bool IsDead => CurrentHealth <= 0;
}