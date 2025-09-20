using System;
using Game.Domain.Enums;

namespace Game.Domain.Entities.Abilities
{
    public class StatusEffect : BaseEntity
    {
        public EffectType Type { get; private set; }
        public int Stacks { get; private set; }
        public string Name { get; private set; }
        
        public event Action OnExpired;
        
        public StatusEffect(EffectType type, int stacks, string name)
        {
            Type = type;
            Stacks = stacks;
            Name = name;
        }
        
        
        public bool IsExpired => Stacks <= 0;
        public bool IsPermanent => Stacks < 0;
        
        public void ReduceStacks(int amount)
        {
            Stacks = Math.Max(0, Stacks - amount);
            if (Stacks <= 0)
            {
                OnExpired?.Invoke();
            }
            NotifyModelUpdated();
        }

        public void IncreaseStacks(int amount)
        {
            if (amount > 0)
            {
                Stacks += amount;
                NotifyModelUpdated();
            }
        }
    }
}