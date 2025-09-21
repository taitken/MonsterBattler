using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct AbilityEffect
    {
        public EffectType Type { get; }
        public int Value { get; }
        public TargetType TargetType { get; }
        public RuneType? AmplifyRuneType { get; }
        public int? AmplifyAmount { get; }

        public AbilityEffect(EffectType type, int value, TargetType targetType, RuneType? amplifyRuneType, int? amplifyAmount)
        {
            Type = type;
            Value = value;
            TargetType = targetType;
            AmplifyRuneType = amplifyRuneType;
            AmplifyAmount = amplifyAmount;  
        }
        
    }
}