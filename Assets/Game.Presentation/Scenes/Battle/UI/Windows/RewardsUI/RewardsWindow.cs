using UnityEngine;
using System.Collections.Generic;
using Game.Domain.Enums;
using Game.Presentation.UI.ButtonUI;

public class RewardsWindow : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RewardOptionUI _rewardOptionPrefab;
    [SerializeField] private ButtonUI _continueButton;
    [SerializeField] private RewardOptionUI _referenceRewardOption; // The positioned instance in the scene
    private Transform _contentParent;
        private List<RewardOptionUI> _currentRewardOptions = new List<RewardOptionUI>();
    private Vector3 _referencePosition;
    private bool _hasInitialized = false;

    void Awake()
    {
        // If no content parent is set, use the canvas as the parent
        if (_contentParent == null && _canvas != null)
            _contentParent = _canvas.transform;
            
        InitializeReferencePosition();
    }
    
    private void InitializeReferencePosition()
    {
        if (!_hasInitialized && _referenceRewardOption != null)
        {
            // Store the reference position from the positioned instance
            _referencePosition = _referenceRewardOption.transform.localPosition;
            
            // Destroy the reference instance
            DestroyImmediate(_referenceRewardOption.gameObject);
            _referenceRewardOption = null;
            
            _hasInitialized = true;
        }
    }

    public void InitializeRewards()
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
            
            // Position them using the reference position with 0.05f margin between each
            var transform = rewardOption.transform;
            Vector3 newPosition = _referencePosition;
            newPosition.y -= i * 130f; // Move each option down by 0.05f units
            transform.localPosition = newPosition;
        }
    }

    public void Show()
    {
        InitializeRewards();
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
