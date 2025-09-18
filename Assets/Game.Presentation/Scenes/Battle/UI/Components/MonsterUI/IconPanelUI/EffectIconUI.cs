using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Enums;
using Game.Application.Interfaces;
using Game.Core;

public class EffectIconUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;

    private Image _iconImage;
    private EffectType _currentEffectType;
    private int _currentValue;
    private IStatusEffectIconProvider _iconProvider;

    void Awake()
    {
        _iconImage = GetComponent<Image>();
        _iconProvider = ServiceLocator.Get<IStatusEffectIconProvider>();
    }

    public void SetEffect(EffectType effectType, int value)
    {
        _currentEffectType = effectType;
        _currentValue = value;
        
        // Set the appropriate sprite based on effect type using provider service
        Sprite effectSprite = _iconProvider?.GetEffectSprite(effectType);

        if (effectSprite != null)
        {
            _iconImage.sprite = effectSprite;
            gameObject.SetActive(true);

            // Update the number text (show value if > 1, otherwise hide)
            if (_numberText != null)
            {
                _numberText.text = value > 1 ? value.ToString() : "";
            }
        }
        else
        {
            // Hide the entire icon if effect type is not supported by provider
            gameObject.SetActive(false);
        }
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public EffectType EffectType => _currentEffectType;
    public int Value => _currentValue;
}
