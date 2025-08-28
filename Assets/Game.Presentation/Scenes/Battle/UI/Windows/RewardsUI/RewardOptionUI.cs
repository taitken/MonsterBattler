using System;
using Game.Application.Repositories;
using Game.Core;
using Game.Domain.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class RewardOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float hoverScale = 1.005f;
    private Color hoverTintColor = new(1f, 0.90f, 0.61f, 1f);  
    private float transitionDuration = 0.1f;
    
    private Vector3 _originalScale;
    private Color _originalColor;
    private RectTransform _rectTransform;
    private Image _image;
    private IPlayerDataRepository _playerDataRepo;
    private ResourceType _rewardType;
    private int _rewardAmount;
    
    public event Action<RewardOptionUI> OnRewardClaimed;
    
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _playerDataRepo = ServiceLocator.Get<IPlayerDataRepository>();
        
        // Store original values
        _originalScale = _rectTransform.localScale;
        if (_image != null)
            _originalColor = _image.color;
    }
    
    public void Initialize(ResourceType rewardType, int amount)
    {
        _rewardType = rewardType;
        _rewardAmount = amount;
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
        Color startColor = _image != null ? _image.color : Color.white;
        Vector3 targetScale = _originalScale * hoverScale;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float progress = elapsedTime / transitionDuration;
            
            // Smooth transition using easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Scale transition
            _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);
            
            // Color transition
            if (_image != null)
                _image.color = Color.Lerp(startColor, hoverTintColor, easedProgress);
            
            yield return null;
        }

        // Ensure final values are set
        _rectTransform.localScale = targetScale;
        if (_image != null)
            _image.color = hoverTintColor;
    }

    private System.Collections.IEnumerator TransitionToNormal()
    {
        float elapsedTime = 0f;
        Vector3 startScale = _rectTransform.localScale;
        Color startColor = _image != null ? _image.color : Color.white;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float progress = elapsedTime / transitionDuration;
            
            // Smooth transition using easing
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Scale transition
            _rectTransform.localScale = Vector3.Lerp(startScale, _originalScale, easedProgress);
            
            // Color transition
            if (_image != null)
                _image.color = Color.Lerp(startColor, _originalColor, easedProgress);
            
            yield return null;
        }

        // Ensure final values are set
        _rectTransform.localScale = _originalScale;
        if (_image != null)
            _image.color = _originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Grant the reward
        var playerResources = _playerDataRepo.GetPlayerResources();
        playerResources.AddResource(_rewardType, _rewardAmount);
        
        // Notify parent that this reward was claimed
        OnRewardClaimed?.Invoke(this);
        
        // Hide this reward option
        gameObject.SetActive(false);
    }
}
