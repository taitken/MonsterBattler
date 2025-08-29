using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RuneSlotMachineUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private RectTransform _firstContainer;
    [SerializeField] private RectTransform _secondContainer;
    [SerializeField] private RectTransform _thirdContainer;
    
    [Header("Active Position References")]
    [SerializeField] private RuneIconGroupUI _firstRuneIconGroup;
    [SerializeField] private RuneIconGroupUI _secondRuneIconGroup;
    [SerializeField] private RuneIconGroupUI _thirdRuneIconGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float _spinDuration = 2f;
    [SerializeField] private float _spinSpeed = 500f;
    [SerializeField] private Ease _spinEase = Ease.OutQuart;
    [SerializeField] private float _staggerDelay = 0.2f;
    
    [Header("Pizzazz Settings")]
    [SerializeField] private float _windupDistance = 30f; // How far up to move before spinning down
    [SerializeField] private float _windupDuration = 0.3f; // Duration of windup animation
    private float _overshootDistance = 20f; // How far past final position to overshoot
    [SerializeField] private float _settleDuration = 0.4f; // Duration of settle back animation

    private const int TOTAL_GROUPS_PER_CONTAINER = 6;
    private const int EXTRA_GROUPS_TO_CREATE = 5;
    private const int BUFFER_ICONS_COUNT = 2; // Add 2 icons at start and 2 at end for visual buffer
    private const float GROUP_SPACING = 125f;
    
    private List<RuneIconGroupUI> _firstContainerGroups = new List<RuneIconGroupUI>();
    private List<RuneIconGroupUI> _secondContainerGroups = new List<RuneIconGroupUI>();
    private List<RuneIconGroupUI> _thirdContainerGroups = new List<RuneIconGroupUI>();
    
    private bool _isSpinning = false;

    void Start()
    {
        InitializeSlotMachine();
    }

    public void StartSpin(int[] targetIndices = null)
    {
        if (_isSpinning) return;
        
        // If no target indices provided, use random ones
        if (targetIndices == null || targetIndices.Length != 3)
        {
            targetIndices = new int[3];
            for (int i = 0; i < 3; i++)
            {
                targetIndices[i] = Random.Range(0, TOTAL_GROUPS_PER_CONTAINER);
            }
        }
        
        StartCoroutine(SpinSlotMachine(targetIndices));
    }

    private void InitializeSlotMachine()
    {
        // Initialize each container
        InitializeContainer(_firstContainer, _firstRuneIconGroup, _firstContainerGroups);
        InitializeContainer(_secondContainer, _secondRuneIconGroup, _secondContainerGroups);
        InitializeContainer(_thirdContainer, _thirdRuneIconGroup, _thirdContainerGroups);
    }

    private void InitializeContainer(RectTransform container, RuneIconGroupUI referenceGroup, List<RuneIconGroupUI> groupList)
    {
        // Get reference position (should be the "active" center position)
        Vector3 referencePos = referenceGroup.GetComponent<RectTransform>().anchoredPosition;
        
        // Create buffer icons at the start (duplicates of last few icons)
        for (int i = 0; i < BUFFER_ICONS_COUNT; i++)
        {
            GameObject bufferGroup = Instantiate(referenceGroup.gameObject, container);
            RuneIconGroupUI bufferUI = bufferGroup.GetComponent<RuneIconGroupUI>();
            groupList.Add(bufferUI);
        }
        
        // Add the reference group and create the main groups
        groupList.Add(referenceGroup);
        for (int i = 0; i < EXTRA_GROUPS_TO_CREATE; i++)
        {
            GameObject newGroup = Instantiate(referenceGroup.gameObject, container);
            RuneIconGroupUI groupUI = newGroup.GetComponent<RuneIconGroupUI>();
            groupList.Add(groupUI);
        }
        
        // Create buffer icons at the end (duplicates of first few icons)
        for (int i = 0; i < BUFFER_ICONS_COUNT; i++)
        {
            GameObject bufferGroup = Instantiate(referenceGroup.gameObject, container);
            RuneIconGroupUI bufferUI = bufferGroup.GetComponent<RuneIconGroupUI>();
            groupList.Add(bufferUI);
        }
        
        // Position all groups including buffers
        PositionGroupsInContainer(groupList, referencePos);
    }

    private void PositionGroupsInContainer(List<RuneIconGroupUI> groups, Vector3 referencePos)
    {
        // The reference group should be at index BUFFER_ICONS_COUNT (after the buffer icons)
        int referenceIndex = BUFFER_ICONS_COUNT;
        
        for (int i = 0; i < groups.Count; i++)
        {
            Vector3 pos = groups[i].GetComponent<RectTransform>().anchoredPosition;
            // Position relative to the reference group, with buffer icons extending beyond the normal range
            float offsetFromReference = (referenceIndex + (TOTAL_GROUPS_PER_CONTAINER - i)) * GROUP_SPACING;
            pos.y = referencePos.y + offsetFromReference;
            groups[i].GetComponent<RectTransform>().anchoredPosition = pos;
        }
    }

    private IEnumerator SpinSlotMachine(int[] targetIndices)
    {
        _isSpinning = true;
        
        // Start spinning all three containers with staggered timing, each targeting specific indices
        StartContainerSpin(_firstContainerGroups, 0f, targetIndices[0]);
        yield return new WaitForSeconds(_staggerDelay);
        
        StartContainerSpin(_secondContainerGroups, 0f, targetIndices[1]);
        yield return new WaitForSeconds(_staggerDelay);
        
        StartContainerSpin(_thirdContainerGroups, 0f, targetIndices[2]);
        
        // Wait for all spins to complete (including windup, spinning, and settle)
        float totalAnimationTime = _windupDuration + _spinDuration + _settleDuration;
        yield return new WaitForSeconds(totalAnimationTime + (_staggerDelay * 2));
        
        _isSpinning = false;
    }

    private void StartContainerSpin(List<RuneIconGroupUI> groups, float delay, int targetIndex)
    {
        StartCoroutine(AnimateContainerSpin(groups, delay, targetIndex));
    }
    
    private IEnumerator AnimateContainerSpin(List<RuneIconGroupUI> groups, float delay, int targetIndex)
    {
        yield return new WaitForSeconds(delay);
        
        // Store original positions before any animation
        Vector3[] originalPositions = new Vector3[groups.Count];
        for (int i = 0; i < groups.Count; i++)
        {
            originalPositions[i] = groups[i].GetComponent<RectTransform>().anchoredPosition;
        }
        
        // 1. WINDUP: Move everything up slightly for dramatic effect
        var windupSequence = DOTween.Sequence();
        for (int i = 0; i < groups.Count; i++)
        {
            int index = i;
            Vector3 windupPos = originalPositions[index];
            windupPos.y += _windupDistance;
            
            windupSequence.Join(groups[index].GetComponent<RectTransform>().DOLocalMoveY(windupPos.y, _windupDuration)
                .SetEase(Ease.OutBack));
        }
        yield return windupSequence.WaitForCompletion();
        
        // 2. MAIN SPINNING ANIMATION
        const int FULL_ROTATIONS = 2; // Changed to 2 rotations, third is handled by target positioning
        float containerHeight = GROUP_SPACING * TOTAL_GROUPS_PER_CONTAINER;
        
        // Calculate timing: both rotations are equal speed now
        float rotationDuration = _spinDuration * 0.3f; // 40% each for 2 rotations, 20% for target positioning
        
        for (int rotation = 0; rotation < FULL_ROTATIONS; rotation++)
        {
            // Store initial positions for this rotation
            Vector3[] startPositions = new Vector3[groups.Count];
            for (int i = 0; i < groups.Count; i++)
            {
                startPositions[i] = groups[i].GetComponent<RectTransform>().anchoredPosition;
            }
            
            // All rotations use linear easing for consistent speed
            Ease rotationEase = Ease.Linear;
            
            // Calculate target position for this rotation
            var sequence = DOTween.Sequence();
            for (int i = 0; i < groups.Count; i++)
            {
                int index = i;
                Vector3 targetPos = startPositions[index];
                targetPos.y -= containerHeight; // Normal rotation distance for all rotations
                
                sequence.Join(groups[index].GetComponent<RectTransform>().DOLocalMoveY(targetPos.y, rotationDuration)
                    .SetEase(rotationEase));
            }
            
            yield return sequence.WaitForCompletion();
            
            // Reset positions for continuous loop (except on the last rotation)
            if (rotation < FULL_ROTATIONS)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    Vector3 resetPos = groups[i].GetComponent<RectTransform>().anchoredPosition;
                    resetPos.y += containerHeight;
                    groups[i].GetComponent<RectTransform>().anchoredPosition = resetPos;
                }
            }
        }
        
        // 3. FINAL TARGETED ROTATION with overshoot
        float targetRotationDuration = _spinDuration * 0.4f; // Remaining 20% of spin duration
        
        // Calculate target positions with overshoot for the final rotation
        Vector3[] targetPositionsWithOvershoot = new Vector3[groups.Count];
        // Since we reset after step 2, we're back at original positions
        // Move down by containerHeight (one full rotation) plus target offset
        float additionalOffset = (targetIndex - 1) * GROUP_SPACING;
        
        for (int i = 0; i < groups.Count; i++)
        {
            Vector3 currentPos = groups[i].GetComponent<RectTransform>().anchoredPosition;
            targetPositionsWithOvershoot[i] = currentPos;
            targetPositionsWithOvershoot[i].y -= containerHeight - additionalOffset + _overshootDistance;
        }
        
        // Animate to target positions with overshoot
        var targetSequence = DOTween.Sequence();
        for (int i = 0; i < groups.Count; i++)
        {
            int index = i;
            targetSequence.Join(groups[index].GetComponent<RectTransform>().DOLocalMoveY(targetPositionsWithOvershoot[index].y, targetRotationDuration)
                .SetEase(_spinEase));
        }
        
        yield return targetSequence.WaitForCompletion();
        
        // 4. SETTLE: Bounce back to correct positions without overshoot
        Vector3[] correctFinalPositions = new Vector3[groups.Count];
        
        for (int i = 0; i < groups.Count; i++)
        {
            // Remove overshoot from current positions
            correctFinalPositions[i] = groups[i].GetComponent<RectTransform>().anchoredPosition;
            correctFinalPositions[i].y -= _overshootDistance - 10f;
        }
        
        // Settle back to the correct positions
        var settleSequence = DOTween.Sequence();
        for (int i = 0; i < groups.Count; i++)
        {
            int index = i;
            
            settleSequence.Join(groups[index].GetComponent<RectTransform>().DOLocalMoveY(correctFinalPositions[index].y, _settleDuration)
                .SetEase(Ease.OutBounce));
        }
        
        yield return settleSequence.WaitForCompletion();
    }

    // Public method to trigger spin (can be called from buttons, etc.)
    public void TriggerSpin()
    {
        StartSpin();
    }
    
    // Method to get the currently active rune groups (those in center position)
    // These are always the selectable groups, never the buffer icons
    public RuneIconGroupUI[] GetActiveRuneGroups()
    {
        return new RuneIconGroupUI[] { _firstRuneIconGroup, _secondRuneIconGroup, _thirdRuneIconGroup };
    }
    
    // Method to get selectable groups from a container (excludes buffer icons)
    public List<RuneIconGroupUI> GetSelectableGroups(int containerIndex)
    {
        List<RuneIconGroupUI> sourceList = null;
        switch (containerIndex)
        {
            case 0: sourceList = _firstContainerGroups; break;
            case 1: sourceList = _secondContainerGroups; break;
            case 2: sourceList = _thirdContainerGroups; break;
            default: return new List<RuneIconGroupUI>();
        }
        
        // Return only the selectable groups (skip buffer icons at start and end)
        List<RuneIconGroupUI> selectableGroups = new List<RuneIconGroupUI>();
        for (int i = BUFFER_ICONS_COUNT; i < sourceList.Count - BUFFER_ICONS_COUNT; i++)
        {
            selectableGroups.Add(sourceList[i]);
        }
        return selectableGroups;
    }
}
