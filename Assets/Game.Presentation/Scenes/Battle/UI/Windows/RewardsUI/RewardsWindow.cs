using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Game.Domain.Enums;
using Game.Domain.Structs;
using Game.Presentation.UI.ButtonUI;

public class RewardsWindow : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RewardOptionUI _rewardOptionPrefab;
    [SerializeField] private ButtonUI _continueButton;
    private Transform _contentParent;
    
    private List<RewardOptionUI> _currentRewardOptions = new List<RewardOptionUI>();

    void Awake()
    {
        // If no content parent is set, use the canvas as the parent
        if (_contentParent == null && _canvas != null)
            _contentParent = _canvas.transform;
    }

    public void InitializeRewards(BattleResult battleResult)
    {
        ClearExistingRewards();
        CreateRewardOptions();
    }

    private void ClearExistingRewards()
    {
        foreach (var rewardOption in _currentRewardOptions)
        {
            if (rewardOption != null)
                DestroyImmediate(rewardOption.gameObject);
        }
        _currentRewardOptions.Clear();
    }

    private void CreateRewardOptions()
    {
        if (_rewardOptionPrefab == null || _contentParent == null)
            return;
            
        // Define different reward options
        var rewardOptions = new[]
        {
            (ResourceType.Gold, 50),
            (ResourceType.Experience, 25), 
            (ResourceType.Health, 20)
        };
        
        for (int i = 0; i < rewardOptions.Length; i++)
        {
            var rewardOption = Instantiate(_rewardOptionPrefab, _contentParent);
            var (rewardType, amount) = rewardOptions[i];
            
            // Initialize the reward option
            rewardOption.Initialize(rewardType, amount);
            rewardOption.OnRewardClaimed += OnRewardClaimed;
            _currentRewardOptions.Add(rewardOption);
            
            // Position them vertically with even spacing
            var rectTransform = rewardOption.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Position: top (0.75), middle (0.5), bottom (0.25) of parent
                float yPosition = 0.65f - (i * 0.15f);
                rectTransform.anchorMin = new Vector2(0.5f, yPosition);
                rectTransform.anchorMax = new Vector2(0.5f, yPosition);
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }

    public void Show(BattleResult battleResult)
    {
        InitializeRewards(battleResult);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearExistingRewards();
    }
    
    private void OnRewardClaimed(RewardOptionUI claimedReward)
    {
        // Remove the claimed reward from our list
        _currentRewardOptions.Remove(claimedReward);
    }
    
    /// <summary>
    /// Sets up the continue button with an external callback
    /// </summary>
    public void SetupContinueButton(System.Action onContinueClicked)
    {
        if (_continueButton != null)
        {
            _continueButton.RemoveAllListeners();
            _continueButton.AddListener(onContinueClicked);
        }
    }
}
