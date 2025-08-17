using System;
using Game.Domain.Enums;

namespace Game.Domain.Entities.Abilities
{
    public class StatusEffect : BaseEntity
    {
        public EffectType Type { get; private set; }
        public int Value { get; private set; }
        public int RemainingDuration { get; private set; }
        public string Name { get; private set; }
        
        public event Action OnExpired;
        
        public StatusEffect(EffectType type, int value, int duration, string name)
        {
            Type = type;
            Value = value;
            RemainingDuration = duration;
            Name = name;
        }
        
        public void ProcessTurn()
        {
            if (RemainingDuration > 0)
            {
                RemainingDuration--;
                if (RemainingDuration <= 0)
                {
                    OnExpired?.Invoke();
                }
            }
        }
        
        public bool IsExpired => RemainingDuration <= 0;
        public bool IsPermanent => RemainingDuration < 0;
    }
}