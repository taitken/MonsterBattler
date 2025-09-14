using Game.Application.Repositories;
using Game.Core;
using Game.Domain.Enums;
using UnityEngine;
using UnityEngine.UI;

public class ResourceContainerPanel : MonoBehaviour
{
    [Header("Button Icons")]
    [SerializeField] private GameObject _backpackIcon;
    [Header("Resource Icons")]
    [SerializeField] private ResourceIconUI _goldIcon;
    [SerializeField] private ResourceIconUI _experienceIcon;
    [SerializeField] private ResourceIconUI _healthIcon;
    [SerializeField] private ResourceIconUI _manaIcon;
    [SerializeField] private ResourceIconUI _cardIcon;
    [Header("Windows")]
    [SerializeField] private BackpackWindow _backpackWindow;

    private IPlayerDataRepository _playerDataRepo;

    void Awake()
    {
        _playerDataRepo = ServiceLocator.Get<IPlayerDataRepository>();
        _playerDataRepo.GetPlayerResources().OnResourceChanged += UpdateResourceIcons;
        
        // Initialize UI with current values
        InitializeResourceIcons();
        
        // Initialize backpack UI
        InitializeBackpackUI();
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
        if (_cardIcon != null)
            _cardIcon.UpdateValue(resources.Card.ToString());
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
            case ResourceType.Card:
                _cardIcon?.UpdateValue(value.ToString());
                break;
        }
    }

    private void InitializeBackpackUI()
    {
        // Hide the backpack window by default
        if (_backpackWindow != null)
        {
            _backpackWindow.gameObject.SetActive(false);
        }

        // Set up the backpack icon click handler
        if (_backpackIcon != null)
        {
            var button = _backpackIcon.GetComponent<Button>();
            if (button == null)
            {
                // If no Button component, add one
                button = _backpackIcon.AddComponent<Button>();
            }
            
            button.onClick.AddListener(OnBackpackIconClicked);
        }
    }

    private void OnBackpackIconClicked()
    {
        if (_backpackWindow != null)
        {
            bool isCurrentlyActive = _backpackWindow.gameObject.activeSelf;
            
            if (!isCurrentlyActive)
            {
                // Initialize backpack display before showing
                _backpackWindow.InitializeBackpackDisplay();
            }
            
            _backpackWindow.gameObject.SetActive(!isCurrentlyActive);
        }
    }
}
