using Game.Domain.Enums;

namespace Game.Application.Interfaces
{
    public interface IEffectPropertyService
    {
        EffectProperties GetProperties(EffectType effectType);
        bool HasProperty(EffectType effectType, EffectProperties property);
        bool IsBuff(EffectType effectType);
        bool IsDebuff(EffectType effectType);
        bool IsTransient(EffectType effectType);
        bool IsPermanent(EffectType effectType);
        bool HasXValue(EffectType effectType);
    }
}