using UnityEngine;
using UnityEngine.UI;
using Game.Domain.Enums;
using Game.Core;
using Game.Application.Interfaces;
using System.Collections;
using System.Collections.Generic;

public class RuneIconGroupUI : MonoBehaviour
{
    [SerializeField] private Image _glowSprite;
    [SerializeField] private Image _singleRune;
    [SerializeField] private List<Image> _dobuleRune;
    [SerializeField] private List<Image> _tripleRune;
    [SerializeField] private List<Image> _quadRune;
    private const float _glowScale = 1.5f;
    private const float _flashDuration = 0.4f;

    private IRuneIconProvider _runeIconProvider;
    private Image _singleRuneGlow;
    private List<Image> _doubleRuneGlow = new List<Image>();
    private List<Image> _tripleRuneGlow = new List<Image>();
    private List<Image> _quadRuneGlow = new List<Image>();

    void Start()
    {
        _runeIconProvider = ServiceLocator.Get<IRuneIconProvider>();

        // Hide the template glow sprite
        _glowSprite.gameObject.SetActive(false);

        CreateGlowImages();
        HideAllImages();
    }

    public void UpdateSprite(List<RuneType> runeTypes)
    {
        HideAllImages();
        switch (runeTypes.Count)
        {
            case 0:
                SetSprite(_singleRune, RuneType.Plain);
                UpdateGlowColor(_singleRuneGlow, RuneType.Plain);
                break;
            case 1:
                SetSprite(_singleRune, runeTypes[0]);
                UpdateGlowColor(_singleRuneGlow, runeTypes[0]);
                break;
            case 2:
                UpdateSprites(_dobuleRune,runeTypes);
                UpdateGlowColors(_doubleRuneGlow, runeTypes);
                break;
            case 3:
                UpdateSprites(_tripleRune, runeTypes);
                UpdateGlowColors(_tripleRuneGlow, runeTypes);
                break;
            case 4:
                UpdateSprites(_quadRune,  runeTypes);
                UpdateGlowColors(_quadRuneGlow, runeTypes);
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
        if (_singleRuneGlow != null) _singleRuneGlow.gameObject.SetActive(false);

        foreach (var image in _dobuleRune)
        {
            image.gameObject.SetActive(false);
        }
        foreach (var glow in _doubleRuneGlow)
        {
            glow.gameObject.SetActive(false);
        }

        foreach (var image in _tripleRune)
        {
            image.gameObject.SetActive(false);
        }
        foreach (var glow in _tripleRuneGlow)
        {
            glow.gameObject.SetActive(false);
        }

        foreach (var image in _quadRune)
        {
            image.gameObject.SetActive(false);
        }
        foreach (var glow in _quadRuneGlow)
        {
            glow.gameObject.SetActive(false);
        }
    }

    private void SetSprite(Image icon, RuneType type)
    {
        var sprite = _runeIconProvider.GetRuneSprite(type);
        icon.sprite = sprite;
        icon.gameObject.SetActive(true);
    }

    private void CreateGlowImages()
    {
        _singleRuneGlow = CreateGlowImage(_singleRune);

        foreach (var rune in _dobuleRune)
        {
            _doubleRuneGlow.Add(CreateGlowImage(rune));
        }

        foreach (var rune in _tripleRune)
        {
            _tripleRuneGlow.Add(CreateGlowImage(rune));
        }

        foreach (var rune in _quadRune)
        {
            _quadRuneGlow.Add(CreateGlowImage(rune));
        }
    }

    private Image CreateGlowImage(Image originalRune)
    {
        GameObject glowObject = Instantiate(_glowSprite.gameObject);
        glowObject.name = $"{originalRune.name}_Glow";
        glowObject.transform.SetParent(originalRune.transform.parent, false);
        glowObject.transform.SetSiblingIndex(originalRune.transform.GetSiblingIndex());

        Image glowImage = glowObject.GetComponent<Image>();

        RectTransform glowRect = glowImage.rectTransform;
        RectTransform originalRect = originalRune.rectTransform;

        glowRect.anchorMin = originalRect.anchorMin;
        glowRect.anchorMax = originalRect.anchorMax;
        glowRect.anchoredPosition = originalRect.anchoredPosition;
        glowRect.sizeDelta = originalRect.sizeDelta * _glowScale;

        glowImage.color = Color.blue;
        glowImage.gameObject.SetActive(true);

        return glowImage;
    }

    private void UpdateGlowColor(Image glow, RuneType runeType)
    {
        if (glow != null)
        {
            glow.color = _runeIconProvider.GetRuneGlowColor(runeType);
        }
    }

    private void UpdateGlowColors(List<Image> glows, List<RuneType> runeTypes)
    {
        for (int i = 0; i < runeTypes.Count && i < glows.Count; i++)
        {
            UpdateGlowColor(glows[i], runeTypes[i]);
        }
    }

    public void FlashGlow()
    {
        StartCoroutine(FlashGlowCoroutine());
    }

    private IEnumerator FlashGlowCoroutine()
    {
        const float halfDuration = _flashDuration / 1.3f;

        // Get all active glow images
        List<Image> activeGlows = GetActiveGlowImages();

        // Set initial state - make glows visible but transparent
        foreach (var glow in activeGlows)
        {
            glow.gameObject.SetActive(true);
            Color color = glow.color;
            color.a = 0f;
            glow.color = color;
        }

        // Fade in (first half)
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float alpha = elapsed / halfDuration;
            SetGlowAlpha(activeGlows, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we hit full alpha at halfway point
        SetGlowAlpha(activeGlows, 1f);

        // Fade out (second half)
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float alpha = 1f - (elapsed / halfDuration);
            SetGlowAlpha(activeGlows, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide glows at the end
        foreach (var glow in activeGlows)
        {
            glow.gameObject.SetActive(false);
        }
    }

    private List<Image> GetActiveGlowImages()
    {
        List<Image> activeGlows = new List<Image>();

        // Check which rune configuration is currently active and add corresponding glows
        if (_singleRune.gameObject.activeSelf && _singleRuneGlow != null)
        {
            activeGlows.Add(_singleRuneGlow);
        }

        for (int i = 0; i < _dobuleRune.Count; i++)
        {
            if (_dobuleRune[i].gameObject.activeSelf && i < _doubleRuneGlow.Count)
            {
                activeGlows.Add(_doubleRuneGlow[i]);
            }
        }

        for (int i = 0; i < _tripleRune.Count; i++)
        {
            if (_tripleRune[i].gameObject.activeSelf && i < _tripleRuneGlow.Count)
            {
                activeGlows.Add(_tripleRuneGlow[i]);
            }
        }

        for (int i = 0; i < _quadRune.Count; i++)
        {
            if (_quadRune[i].gameObject.activeSelf && i < _quadRuneGlow.Count)
            {
                activeGlows.Add(_quadRuneGlow[i]);
            }
        }

        return activeGlows;
    }

    private void SetGlowAlpha(List<Image> glows, float alpha)
    {
        foreach (var glow in glows)
        {
            Color color = glow.color;
            color.a = alpha;
            glow.color = color;
        }
    }
}
