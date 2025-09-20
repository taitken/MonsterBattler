using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using System;

public class IconPanelUI : MonoBehaviour
{
    [SerializeField] private EffectIconUI effectIconPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int iconsPerRow = 5;

    private Dictionary<EffectType, EffectIconUI> _activeIcons = new();
    private Dictionary<StatusEffect, Action> _subscriptions = new();

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
        if (statusEffects == null)
        {
            ClearAllIcons();
            return;
        }

        var effectsList = statusEffects.ToList();

        // Remove subscriptions for effects that are no longer present
        var effectsToUnsubscribe = _subscriptions.Keys.Where(effect => !effectsList.Contains(effect)).ToList();
        foreach (var effect in effectsToUnsubscribe)
        {
            UnsubscribeFromEffect(effect);
        }

        // Remove icons for effect types that are no longer present
        var currentEffectTypes = effectsList.Select(e => e.Type).Distinct().ToHashSet();
        var iconsToRemove = _activeIcons.Keys.Where(type => !currentEffectTypes.Contains(type)).ToList();
        foreach (var type in iconsToRemove)
        {
            RemoveEffectIcon(type);
        }

        // Update or create icons for each effect
        foreach (var effect in effectsList)
        {
            // Subscribe to this effect if not already subscribed
            if (!_subscriptions.ContainsKey(effect))
            {
                SubscribeToEffect(effect);
            }

            // Update or create the icon for this effect type
            UpdateOrCreateEffectIcon(effect.Type, effect.Stacks);
        }
    }

    private void SubscribeToEffect(StatusEffect effect)
    {
        Action updateAction = () => UpdateEffectIcon(effect.Type, effect.Stacks);
        effect.OnModelUpdated += updateAction;
        _subscriptions[effect] = updateAction;
    }

    private void UnsubscribeFromEffect(StatusEffect effect)
    {
        if (_subscriptions.TryGetValue(effect, out var action))
        {
            effect.OnModelUpdated -= action;
            _subscriptions.Remove(effect);
        }
    }

    private void UpdateOrCreateEffectIcon(EffectType effectType, int stacks)
    {
        if (_activeIcons.TryGetValue(effectType, out var existingIcon))
        {
            // Update existing icon
            existingIcon.SetEffect(effectType, stacks);
        }
        else
        {
            // Create new icon
            CreateEffectIcon(effectType, stacks);
        }
    }

    private void CreateEffectIcon(EffectType effectType, int stacks)
    {
        if (effectIconPrefab == null) return;

        EffectIconUI newIcon = Instantiate(effectIconPrefab, transform);
        newIcon.SetEffect(effectType, stacks);

        _activeIcons[effectType] = newIcon;
    }

    private void UpdateEffectIcon(EffectType effectType, int stacks)
    {
        if (_activeIcons.TryGetValue(effectType, out var icon))
        {
            icon.SetEffect(effectType, stacks);
        }
    }

    private void RemoveEffectIcon(EffectType effectType)
    {
        if (_activeIcons.TryGetValue(effectType, out var icon))
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
            _activeIcons.Remove(effectType);
        }
    }

    private void ClearAllIcons()
    {
        // Unsubscribe from all effects
        foreach (var effect in _subscriptions.Keys.ToList())
        {
            UnsubscribeFromEffect(effect);
        }

        // Destroy all icons
        foreach (var icon in _activeIcons.Values)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }

        _activeIcons.Clear();
    }

    void OnDestroy()
    {
        // Clean up subscriptions when component is destroyed
        ClearAllIcons();
    }

}
