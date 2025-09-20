using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct AbilityEffect
    {
        public EffectType Type { get; }
        public int Value { get; }
        public TargetType TargetType { get; }
        
        public AbilityEffect(EffectType type, int value, TargetType targetType)
        {
            Type = type;
            Value = value;
            TargetType = targetType;
        }
        
    }
}