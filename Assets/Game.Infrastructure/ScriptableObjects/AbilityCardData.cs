using System.Collections.Generic;
using UnityEngine;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Ability Card", menuName = "Cards/Ability Card Data")]
    public class AbilityCardData : ScriptableObject
    {
        [Header("Basic Info")]
        public string cardName;
        [TextArea(3, 5)]
        public string description;
        public Sprite cardArt;
        
        [Header("Effects")]
        public List<AbilityEffectData> effects = new();
        public List<RuneType> runes = new();
        
        [Header("Audio")]
        public AudioClip playSound;
        
        public AbilityCard ToEntity()
        {
            var abilityEffects = new List<AbilityEffect>();
            foreach (var effectData in effects)
            {
                abilityEffects.Add(new AbilityEffect(
                    effectData.effectType,
                    effectData.value,
                    effectData.targetType
                ));
            }

            return new AbilityCard(
                name: cardName,
                description: description,
                effects: abilityEffects,
                runes: runes
            );
        }
    }
    
    [System.Serializable]
    public struct AbilityEffectData
    {
        public EffectType effectType;
        public int value;
        public TargetType targetType;
    }
}