using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core;
using Game.Presentation.Core.Helpers;
using Game.Presentation.Scenes.Battle.UI.Windows.RewardsUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Application.DTOs.Rewards;

[RequireComponent(typeof(RectTransform))]
public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _text;

    private const float HoverScale = 1.005f;
    private const float TransitionDuration = 0.1f;
    private const float LightenFactor = 0.5f;
    private Color _hoverTintColor;
    private Vector3 _originalScale;
    private Color _originalColor;
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private IEventBus _eventBus;
    private IResourceIconProvider _resourceIconProvider;
    private Reward _reward;
    private RewardClickHandler _clickHandler;
    
    public event Action<RewardOptionUI> OnRewardClaimed;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _backgroundImage = GetComponent<Image>();
        _eventBus = ServiceLocator.Get<IEventBus>();
        _resourceIconProvider = ServiceLocator.Get<IResourceIconProvider>();

        // Store original values
        _originalScale = _rectTransform.localScale;
        if (_backgroundImage != null)
        {
            _originalColor = _backgroundImage.color;
            _hoverTintColor = ColorHelper.Lighten(_originalColor, LightenFactor);
        }
            
    }

    public void Initialize(Reward reward)
    {
        _reward = reward;
        _clickHandler = new RewardClickHandler(_eventBus, this);
        _iconImage.sprite = _resourceIconProvider.GetResourceSprite(reward.Type);
        _text.SetText(reward.DisplayText);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up and darken
        StopAllCoroutines();
        StartCoroutine(TransitionToHover());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back and restore color
        StopAllCoroutines();
        StartCoroutine(TransitionToNormal());
    }

    private System.Collections.IEnumerator TransitionToHover()
    {
        float elapsedTime = 0f;
        Vector3 startScale = _rectTransform.localScale;
        Color startColor = _backgroundImage != null ? _backgroundImage.color : Color.white;
        Vector3 targetScale = _originalScale * HoverScale;

        while (elapsedTime < TransitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float progress = elapsedTime / TransitionDuration;
            
            // Smooth transition using easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Scale transition
            _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            
            // Color transition
            if (_backgroundImage != null)
                _backgroundImage.color = Color.Lerp(startColor, _hoverTintColor, easedProgress);
            
            yield return null;
        }

        // Ensure final values are set
        _rectTransform.localScale = targetScale;
        if (_backgroundImage != null)
            _backgroundImage.color = _hoverTintColor;
    }

    private System.Collections.IEnumerator TransitionToNormal()
    {
        float elapsedTime = 0f;
        Vector3 startScale = _rectTransform.localScale;
        Color startColor = _backgroundImage != null ? _backgroundImage.color : Color.white;

        while (elapsedTime < TransitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float progress = elapsedTime / TransitionDuration;
            
            // Smooth transition using easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Scale transition
            _rectTransform.localScale = Vector3.Lerp(startScale, _originalScale, easedProgress);
            
            // Color transition
            if (_backgroundImage != null)
                _backgroundImage.color = Color.Lerp(startColor, _originalColor, easedProgress);
            
            yield return null;
        }

        // Ensure final values are set
        _rectTransform.localScale = _originalScale;
        if (_backgroundImage != null)
            _backgroundImage.color = _originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _reward.Accept(_clickHandler);
    }

    public void NotifyRewardClaimed()
    {
        // Notify parent that this reward was claimed
        OnRewardClaimed?.Invoke(this);
        
        // Hide this reward option
        gameObject.SetActive(false);
    }
}
