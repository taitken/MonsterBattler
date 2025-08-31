using UnityEngine;
using System.Collections.Generic;
using Game.Presentation.UI.ButtonUI;
using Game.Application.DTOs.Rewards;

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

    public void ShowWithRewards(IEnumerable<Reward> rewards, System.Action onContinueClicked)
    {
        ClearExistingRewards();
        CreateRewardOptions(rewards);
        SetupContinueButton(onContinueClicked);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearExistingRewards();
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

    private void CreateRewardOptions(IEnumerable<Reward> rewards)
    {
        if (_rewardOptionPrefab == null || _contentParent == null || rewards == null)
            return;

        int i = 0;
        foreach (var reward in rewards)
        {
            var rewardOption = Instantiate(_rewardOptionPrefab, _contentParent);
            
            // Initialize the reward option
            rewardOption.Initialize(reward);
            rewardOption.OnRewardClaimed += OnRewardClaimed;
            _currentRewardOptions.Add(rewardOption);
            
            // Position them using the reference position with spacing between each
            var transform = rewardOption.transform;
            Vector3 newPosition = _referencePosition;
            newPosition.y -= i * 130f;
            transform.localPosition = newPosition;
            
            i++;
        }
    }
    
    private void OnRewardClaimed(RewardOptionUI claimedReward)
    {
        _currentRewardOptions.Remove(claimedReward);
    }
    
    private void SetupContinueButton(System.Action onContinueClicked)
    {
        if (_continueButton != null)
        {
            _continueButton.RemoveAllListeners();
            _continueButton.AddListener(onContinueClicked);
        }
    }
}
