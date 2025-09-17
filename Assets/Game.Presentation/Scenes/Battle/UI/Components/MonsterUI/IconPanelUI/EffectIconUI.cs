using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Enums;

public class EffectIconUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Sprite _defendImage;
    [SerializeField] private Sprite _burnImage;
    [SerializeField] private Sprite _weakenImage;
    [SerializeField] private Sprite _poisonImage;

    private Image _iconImage;
    private EffectType _currentEffectType;
    private int _currentValue;

    void Awake()
    {
        _iconImage = GetComponent<Image>();
    }

    public void SetEffect(EffectType effectType, int value)
    {
        _currentEffectType = effectType;
        _currentValue = value;
        
        // Set the appropriate sprite based on effect type
        Sprite effectSprite = GetSpriteForEffectType(effectType);
        
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
            // Hide the entire icon if effect type is not supported
            gameObject.SetActive(false);
        }
    }

    private Sprite GetSpriteForEffectType(EffectType effectType)
    {
        return effectType switch
        {
            EffectType.Block => _defendImage,
            EffectType.Burn => _burnImage,
            EffectType.Poison => _poisonImage,
            _ => _defendImage
        };
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public EffectType EffectType => _currentEffectType;
    public int Value => _currentValue;
}
