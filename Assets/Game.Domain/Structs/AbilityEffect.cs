using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct AbilityEffect
    {
        public EffectType Type { get; }
        public int Value { get; }
        public int Duration { get; }
        public TargetType TargetType { get; }
        
        public AbilityEffect(EffectType type, int value, TargetType targetType, int duration = 0)
        {
            Type = type;
            Value = value;
            TargetType = targetType;
            Duration = duration;
        }
        
        public bool IsInstant => Duration == 0;
        public bool IsTemporary => Duration > 0;
    }
}