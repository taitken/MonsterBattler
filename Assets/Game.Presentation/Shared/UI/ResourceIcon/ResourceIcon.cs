using Game.Application.Interfaces;
using Game.Core;
using Game.Domain.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceIconUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ResourceType _resourceType = ResourceType.Gold;

    [Header("UI Components")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _valueText;

    private IResourceIconProvider _resourceIconProvider;

    void Start()
    {
        _resourceIconProvider = ServiceLocator.Get<IResourceIconProvider>();
        UpdateIcon();
        UpdateValue(0.ToString());
    }

    private void UpdateIcon()
    {
        var sprite = _resourceIconProvider.GetResourceSprite(_resourceType);
        if (sprite != null && _iconImage != null)
            _iconImage.sprite = sprite;
    }

    public void UpdateValue(string newValue)
    {
        _valueText.text = newValue;
    }
}
