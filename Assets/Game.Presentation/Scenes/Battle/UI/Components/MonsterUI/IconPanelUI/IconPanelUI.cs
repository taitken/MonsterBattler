using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

public class IconPanelUI : MonoBehaviour
{
    [SerializeField] private EffectIconUI effectIconPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int iconsPerRow = 5;
    
    private List<EffectIconUI> _activeIcons = new();

    void Awake()
    {
        // Set up grid layout for 5 icons per row
        if (gridLayout != null)
        {
            gridLayout.constraintCount = iconsPerRow;
        }
    }

    public void UpdateStatusEffects(IEnumerable<StatusEffect> statusEffects)
    {
        // Clear existing icons
        ClearAllIcons();

        if (statusEffects == null)
            return;

        // Group effects by type and sum their values
        var effectGroups = statusEffects
            .Where(effect => IsSupportedEffectType(effect.Type))
            .GroupBy(effect => effect.Type)
            .ToDictionary(group => group.Key, group => group.Sum(effect => effect.Value));

        // Create icons for each effect type
        foreach (var kvp in effectGroups)
        {
            CreateEffectIcon(kvp.Key, kvp.Value);
        }
    }

    private void CreateEffectIcon(EffectType effectType, int value)
    {
        if (effectIconPrefab == null) return;

        EffectIconUI newIcon = Instantiate(effectIconPrefab, transform);
        newIcon.SetEffect(effectType, value);
        
        _activeIcons.Add(newIcon);
    }

    private void ClearAllIcons()
    {
        foreach (var icon in _activeIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }
        
        _activeIcons.Clear();
    }

    private bool IsSupportedEffectType(EffectType effectType)
    {
        return effectType == EffectType.Defend ||
               effectType == EffectType.Burn ||
               effectType == EffectType.Weaken ||
               effectType == EffectType.Poison;
    }

}
