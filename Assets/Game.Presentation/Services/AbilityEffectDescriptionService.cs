using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Domain.Structs;
using UnityEngine;

namespace Game.Presentation.Services
{
    public static class AbilityEffectDescriptionService
    {
        private static IRuneIconProvider _runeIconProvider;

        public static void Initialize(IRuneIconProvider runeIconProvider)
        {
            _runeIconProvider = runeIconProvider;
        }

        public static string GenerateDescription(IReadOnlyList<AbilityEffect> effects)
        {
            return GenerateDescription(effects, null);
        }

        public static string GenerateDescription(IReadOnlyList<AbilityEffect> effects, IReadOnlyList<RuneType> flashingRunes)
        {
            if (effects == null || effects.Count == 0)
                return string.Empty;

            var descriptions = effects.Select(effect => GenerateEffectDescription(effect, flashingRunes)).ToList();
            var result = string.Join(". ", descriptions);

            // Debug logging
            Debug.Log($"Generated card description: {result}");

            return result;
        }

        private static string GenerateEffectDescription(AbilityEffect effect)
        {
            return GenerateEffectDescription(effect, null);
        }

        private static string GenerateEffectDescription(AbilityEffect effect, IReadOnlyList<RuneType> flashingRunes)
        {
            var sb = new StringBuilder();

            // Get the action verb and format based on effect type
            var actionText = GetActionText(effect.Type);
            var targetText = GetTargetText(effect.TargetType);

            // Format: Action [RuneIcon] <b>Value</b> Type to <b>Target</b>
            sb.Append(actionText);

            // Check if this effect should be amplified by flashing runes
            bool isTemporarilyAmplified = flashingRunes != null && effect.AmplifyRuneType.HasValue && flashingRunes.Contains(effect.AmplifyRuneType.Value);

            // Add rune icon if this effect has amplification
            if (effect.AmplifyRuneType.HasValue && effect.AmplifyAmount.HasValue)
            {
                var colorHex = ColorUtility.ToHtmlStringRGB(_runeIconProvider.GetRuneGlowColor(effect.AmplifyRuneType.Value));
                sb.Append(" ");
                sb.Append(GetRuneIcon(effect.AmplifyRuneType.Value, effect.AmplifyAmount.Value));
                sb.Append(" <color=#");
                sb.Append(colorHex);
                sb.Append("><size=+4><b>");

                // Show amplified value if temporarily amplified, otherwise show base value
                if (isTemporarilyAmplified)
                {
                    sb.Append(effect.Value + effect.AmplifyAmount.Value); // Add the flat amplify amount
                }
                else
                {
                    sb.Append(effect.Value);
                }

                sb.Append("</b></size></color>");
            }
            else
            {
                sb.Append(" <b>");
                sb.Append(effect.Value);
                sb.Append("</b>");
            }

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

        private static string GetRuneIcon(RuneType runeType)
        {
            return GetRuneIcon(runeType, 1);
        }

        private static string GetRuneIcon(RuneType runeType, int amount)
        {
            if (_runeIconProvider == null)
                return new string('◆', amount); // Fallback if not initialized

            var color = _runeIconProvider.GetRuneGlowColor(runeType);
            var colorHex = ColorUtility.ToHtmlStringRGB(color);

            var symbol = runeType switch
            {
                RuneType.Fire => "♦",      // Fire: diamond
                RuneType.Water => "●",     // Water: circle
                RuneType.Earth => "■",     // Earth: square
                RuneType.Grass => "▲",     // Grass: triangle
                RuneType.Plain => "○",     // Plain: hollow circle
                RuneType.Dark => "▼",      // Dark: inverted triangle
                _ => "◆"
            };

            // Repeat the symbol based on amount and wrap in color tags
            var repeatedSymbol = new string(symbol[0], amount);
            return $"<color=#{colorHex}>{repeatedSymbol}</color>";
        }
    }
}