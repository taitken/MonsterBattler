using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Domain.Services
{
    public static class AbilityEffectDescriptionService
    {
        public static string GenerateDescription(IReadOnlyList<AbilityEffect> effects)
        {
            if (effects == null || effects.Count == 0)
                return string.Empty;

            var descriptions = effects.Select(GenerateEffectDescription).ToList();
            return string.Join(". ", descriptions);
        }

        private static string GenerateEffectDescription(AbilityEffect effect)
        {
            var sb = new StringBuilder();
            
            // Get the action verb and format based on effect type
            var actionText = GetActionText(effect.Type);
            var targetText = GetTargetText(effect.TargetType);
            var durationText = GetDurationText(effect.Duration);
            
            // Format: Action <b>Value</b> Type to <b>Target</b> [for <b>Duration</b> turns]
            sb.Append(actionText);
            sb.Append(" <b>");
            sb.Append(effect.Value);
            sb.Append("</b>");
            
            // Add effect type if needed (for buffs/debuffs)
            var typeText = GetEffectTypeText(effect.Type);
            if (!string.IsNullOrEmpty(typeText))
            {
                sb.Append(" <b>");
                sb.Append(typeText);
                sb.Append("</b>");
            }
            
            sb.Append(" to <b>");
            sb.Append(targetText);
            sb.Append("</b>");
            
            // Add duration if applicable
            if (!string.IsNullOrEmpty(durationText))
            {
                sb.Append(" for <b>");
                sb.Append(effect.Duration);
                sb.Append("</b> ");
                sb.Append(durationText);
            }
            
            return sb.ToString();
        }

        public static string GetActionText(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.Damage => "Deal",
                EffectType.Heal => string.Empty,
                EffectType.Block => "Gain",
                EffectType.Burn => "Apply",
                EffectType.Poison => "Apply",
                EffectType.Fortify => "Gain",
                EffectType.Regenerate => "Gain",
                EffectType.Proliferate => string.Empty,
                EffectType.Amplify => string.Empty,
                EffectType.Frazzled => "Apply",
                EffectType.Luck => "Gain",
                EffectType.Strength => "Gain",
                EffectType.Backlash => "Gain",
                EffectType.Stun => "Apply",
                EffectType.AddRune => "Add",
                _ => "Apply"
            };
        }

        public static string GetEffectTypeText(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.Damage => "Damage",
                EffectType.Heal => "Heal", 
                EffectType.Block => "Defense",
                EffectType.Burn => "Burn",
                EffectType.Poison => "Poison",
                EffectType.Fortify => "Fortification",
                EffectType.Regenerate => "Regeneration",
                EffectType.Proliferate => "Proliferation",
                EffectType.Amplify => "Amplify",
                EffectType.Frazzled => "Frazzle",
                EffectType.Luck => "Luck",
                EffectType.Strength => "Strength",
                EffectType.Backlash => "Backlash",
                EffectType.Stun => "Stun",
                EffectType.AddRune => "Rune",
                _ => string.Empty
            };
        }

        private static string GetTargetText(TargetType targetType)
        {
            return targetType switch
            {
                TargetType.Self => "Self",
                TargetType.SingleEnemy => "an Enemy",
                TargetType.SingleAlly => "an Ally",
                TargetType.AllEnemies => "all Enemies",
                TargetType.AllAllies => "all Allies",
                TargetType.Random => "a Random Target",
                _ => "Target"
            };
        }

        private static string GetDurationText(int duration)
        {
            if (duration <= 0)
                return string.Empty;
                
            return duration == 1 ? "turn" : "turns";
        }
    }
}