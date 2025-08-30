using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Enums;
using Game.Core;
using Game.Application.Interfaces;
using System.Collections.Generic;

public class RuneIconGroupUI : MonoBehaviour
{
    [SerializeField] private Image _singleRune;
    [SerializeField] private List<Image> _dobuleRune;
    [SerializeField] private List<Image> _tripleRune;
    [SerializeField] private List<Image> _quadRune;
    private IRuneIconProvider _runeIconProvider;

    void Start()
    {
        _runeIconProvider = ServiceLocator.Get<IRuneIconProvider>();
        HideAllImages();
    }

    public void UpdateSprite(List<RuneType> runeTypes)
    {
        HideAllImages();
        switch (runeTypes.Count)
        {
            case 0:
                SetSprite(_singleRune, RuneType.Plain);
                break;
            case 1:
                SetSprite(_singleRune, runeTypes[0]);
                break;
            case 2:
                UpdateSprites(_dobuleRune, runeTypes);
                break;
            case 3:
                UpdateSprites(_tripleRune, runeTypes);
                break;
            case 4:
                UpdateSprites(_quadRune, runeTypes);
                break;
        }
    }

    private void UpdateSprites(List<Image> images, List<RuneType> runeTypes)
    {
        for (int i = 0; i < runeTypes.Count && i < images.Count; i++)
        {
            SetSprite(images[i], runeTypes[i]);
        }
    }

    private void HideAllImages()
    {
        _singleRune.gameObject.SetActive(false);
        
        foreach (var image in _dobuleRune)
        {
            image.gameObject.SetActive(false);
        }
        
        foreach (var image in _tripleRune)
        {
            image.gameObject.SetActive(false);
        }
        
        foreach (var image in _quadRune)
        {
            image.gameObject.SetActive(false);
        }
    }

    private void SetSprite(Image icon, RuneType type)
    {
        var sprite = _runeIconProvider.GetRuneSprite(type);
        icon.sprite = sprite;
        icon.gameObject.SetActive(true);
    }
}
