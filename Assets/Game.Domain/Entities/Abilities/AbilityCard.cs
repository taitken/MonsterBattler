using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Enums;
using Game.Domain.Structs;
using Game.Domain.Services;

namespace Game.Domain.Entities.Abilities
{
    public class AbilityCard : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IReadOnlyList<AbilityEffect> Effects { get; private set; }
        public List<RuneType> Runes { get; private set; }
        
        public string GeneratedDescription => AbilityEffectDescriptionService.GenerateDescription(Effects);

        public AbilityCard(string name, string description,
                          IEnumerable<AbilityEffect> effects,
                          List<RuneType> runes)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Name = name;
            Description = description ?? string.Empty;
            Effects = effects?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(effects));
            Runes = runes?.ToList();
        }

        // Copy constructor for deep copying
        public AbilityCard(AbilityCard other) : this(
            other.Name,
            other.Description,
            other.Effects, // AbilityEffect is a struct, so this creates new copies
            other.Runes?.ToList() // Create a new list of runes
        )
        {
            // Preserve the original ID to maintain card identity
            Id = other.Id;
        }
        
        public bool HasEffect(EffectType effectType) => Effects.Any(e => e.Type == effectType);
        
        public IEnumerable<AbilityEffect> GetEffectsOfType(EffectType effectType) => 
            Effects.Where(e => e.Type == effectType);
    }
}