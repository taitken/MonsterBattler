using UnityEngine;
using System.Collections.Generic;

public class RewardsWindow : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RewardOptionUI _rewardOptionPrefab;
    private Transform _contentParent;
    
    private List<RewardOptionUI> _currentRewardOptions = new List<RewardOptionUI>();

    void Awake()
    {
        // If no content parent is set, use the canvas as the parent
        if (_contentParent == null && _canvas != null)
            _contentParent = _canvas.transform;
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
        for (int i = 0; i < 3; i++)
        {
            var rewardOption = Instantiate(_rewardOptionPrefab, _contentParent);
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
}
