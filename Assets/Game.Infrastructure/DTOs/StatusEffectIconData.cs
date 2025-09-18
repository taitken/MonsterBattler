using Game.Domain.Enums;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public class StatusEffectIconData
    {
        public EffectType effectType;
        public Sprite sprite;
    }
}