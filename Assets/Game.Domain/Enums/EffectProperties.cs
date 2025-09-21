using System;

namespace Game.Domain.Enums
{
    [Flags]
    public enum EffectProperties
    {
        None = 0,
        XValue = 1 << 0,        // Effect has variable values
        Transient = 1 << 1,     // Effect has no lasting stacks
        Buff = 1 << 2,          // Effect is beneficial to the target
        Debuff = 1 << 3,        // Effect is detrimental to the target
        Permanent = 1 << 4      // Effect lasts indefinitely
    }
}