using Game.Application.Interfaces;
using Game.Domain.Data;
using Game.Domain.Enums;

namespace Game.Application.Services.Effects
{
    public class EffectPropertyService : IEffectPropertyService
    {
        public EffectProperties GetProperties(EffectType effectType)
        {
            return EffectPropertyDatabase.GetProperties(effectType);
        }

        public bool HasProperty(EffectType effectType, EffectProperties property)
        {
            return EffectPropertyDatabase.HasProperty(effectType, property);
        }

        public bool IsBuff(EffectType effectType)
        {
            return EffectPropertyDatabase.IsBuff(effectType);
        }

        public bool IsDebuff(EffectType effectType)
        {
            return EffectPropertyDatabase.IsDebuff(effectType);
        }

        public bool IsTransient(EffectType effectType)
        {
            return EffectPropertyDatabase.IsTransient(effectType);
        }

        public bool IsPermanent(EffectType effectType)
        {
            return EffectPropertyDatabase.IsPermanent(effectType);
        }

        public bool HasXValue(EffectType effectType)
        {
            return EffectPropertyDatabase.HasXValue(effectType);
        }
    }
}