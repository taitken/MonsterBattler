using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Domain.Entities.Abilities
{
    public class AbilityCard : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyList<AbilityEffect> Effects { get; private set; }
        
        public AbilityCard(string name, string description, 
                          IEnumerable<AbilityEffect> effects)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            
            Name = name;
            Description = description ?? string.Empty;
            Effects = effects?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(effects));
        }
        
        public bool HasEffect(EffectType effectType) => Effects.Any(e => e.Type == effectType);
        
        public IEnumerable<AbilityEffect> GetEffectsOfType(EffectType effectType) => 
            Effects.Where(e => e.Type == effectType);
    }
}