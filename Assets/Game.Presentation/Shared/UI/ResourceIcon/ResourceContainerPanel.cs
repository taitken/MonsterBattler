using Game.Application.Repositories;
using Game.Core;
using Game.Domain.Enums;
using UnityEngine;

public class ResourceContainerPanel : MonoBehaviour
{
    [Header("Resource Icons")]
    [SerializeField] private ResourceIconUI _goldIcon;
    [SerializeField] private ResourceIconUI _experienceIcon;
    [SerializeField] private ResourceIconUI _healthIcon;
    [SerializeField] private ResourceIconUI _manaIcon;

    private IPlayerDataRepository _playerDataRepo;

    void Awake()
    {
        _playerDataRepo = ServiceLocator.Get<IPlayerDataRepository>();
        _playerDataRepo.GetPlayerResources().OnResourceChanged += UpdateResourceIcons;
        
        // Initialize UI with current values
        InitializeResourceIcons();
    }

    void OnDestroy()
    {
        if (_playerDataRepo != null)
        {
            _playerDataRepo.GetPlayerResources().OnResourceChanged -= UpdateResourceIcons;
        }
    }

    private void InitializeResourceIcons()
    {
        var resources = _playerDataRepo.GetPlayerResources();
        
        if (_goldIcon != null)
            _goldIcon.UpdateValue(resources.Gold.ToString());
        if (_experienceIcon != null)
            _experienceIcon.UpdateValue(resources.Experience.ToString());
        if (_healthIcon != null)
            _healthIcon.UpdateValue(resources.Health.ToString());
        if (_manaIcon != null)
            _manaIcon.UpdateValue(resources.Mana.ToString());
    }

    private void UpdateResourceIcons(ResourceType resourceType, int value)
    {
        switch (resourceType)
        {
            case ResourceType.Gold:
                _goldIcon?.UpdateValue(value.ToString());
                break;
            case ResourceType.Experience:
                _experienceIcon?.UpdateValue(value.ToString());
                break;
            case ResourceType.Health:
                _healthIcon?.UpdateValue(value.ToString());
                break;
            case ResourceType.Mana:
                _manaIcon?.UpdateValue(value.ToString());
                break;
        }
    }
}
