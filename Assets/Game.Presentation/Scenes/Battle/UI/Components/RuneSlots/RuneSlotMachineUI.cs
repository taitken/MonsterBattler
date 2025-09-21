using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Game.Application.DTOs;
using Game.Core;
using Game.Application.Interfaces;
using Game.Domain.Entities.Battle;
using Game.Domain.Enums;
using System.Linq;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging;

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
    private float _spinDuration = .7f;
    private float _spinSpeed = 2500f;
    private Ease _spinEase = Ease.OutQuart;
    private float _staggerDelay = 0.2f;
    
    [Header("Pizzazz Settings")]
    private float _windupDistance = 30f; // How far up to move before spinning down
    private float _windupDuration = 0.3f; // Duration of windup animation
    private float _overshootDistance = 20f; // How far past final position to overshoot
    private float _settleDuration = 0.2f; // Duration of settle back animation

    private const int TOTAL_GROUPS_PER_CONTAINER = 6;
    private const int EXTRA_GROUPS_TO_CREATE = 5;
    private const int BUFFER_ICONS_COUNT = 2; // Add 2 icons at start and 2 at end for visual buffer
    private const float GROUP_SPACING = 125f;
    
    private List<RuneIconGroupUI> _firstContainerGroups = new List<RuneIconGroupUI>();
    private List<RuneIconGroupUI> _secondContainerGroups = new List<RuneIconGroupUI>();
    private List<RuneIconGroupUI> _thirdContainerGroups = new List<RuneIconGroupUI>();
    
    // Store starting positions for each container
    private Vector3 _firstContainerStartingPosition;
    private Vector3 _secondContainerStartingPosition;
    private Vector3 _thirdContainerStartingPosition;
    
    private bool _isSpinning = false;
    private IInteractionBarrier _interactionBarrier;
    private IEventBus _eventBus;
    private RuneSlotMachineEntity _currentRuneSlotMachine;

    void Start()
    {
        _interactionBarrier = ServiceLocator.Get<IInteractionBarrier>();
        _eventBus = ServiceLocator.Get<IEventBus>();
        InitializeSlotMachine();
    }

    public void StartSpin(int[] targetIndices = null, BarrierToken? completionToken = null)
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
        
        StartCoroutine(SpinSlotMachine(targetIndices, completionToken));
    }

    private void InitializeSlotMachine()
    {
        // Initialize each container
        InitializeContainer(_firstContainer, _firstRuneIconGroup, _firstContainerGroups);
        InitializeContainer(_secondContainer, _secondRuneIconGroup, _secondContainerGroups);
        InitializeContainer(_thirdContainer, _thirdRuneIconGroup, _thirdContainerGroups);
        
        // Capture starting positions after initialization
        CaptureStartingPositions();
    }
    
    private void CaptureStartingPositions()
    {
        _firstContainerStartingPosition = _firstContainer.anchoredPosition;
        _secondContainerStartingPosition = _secondContainer.anchoredPosition;
        _thirdContainerStartingPosition = _thirdContainer.anchoredPosition;
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
        for (int i = 0; i < groups.Count; i++)
        {
            Vector3 pos = groups[i].GetComponent<RectTransform>().anchoredPosition;
            float offsetFromReference = (TOTAL_GROUPS_PER_CONTAINER - i) * GROUP_SPACING;
            pos.y = referencePos.y + offsetFromReference;
            groups[i].GetComponent<RectTransform>().anchoredPosition = pos;
        }
    }

    private IEnumerator SpinSlotMachine(int[] targetIndices, BarrierToken? completionToken = null)
    {
        _isSpinning = true;
        
        // Start spinning all three containers with staggered timing
        StartCoroutine(SpinContainerToTarget(_firstContainer, _firstContainerStartingPosition, 0f, targetIndices[0]));
        yield return new WaitForSeconds(_staggerDelay);
        
        StartCoroutine(SpinContainerToTarget(_secondContainer, _secondContainerStartingPosition, 0f, targetIndices[1]));
        yield return new WaitForSeconds(_staggerDelay);
        
        StartCoroutine(SpinContainerToTarget(_thirdContainer, _thirdContainerStartingPosition, 0f, targetIndices[2]));
        
        // Wait for all spins to complete
        float totalAnimationTime = _windupDuration + _spinDuration + _settleDuration;
        yield return new WaitForSeconds(totalAnimationTime + (_staggerDelay * 2));

        _isSpinning = false;

        // Flash glow for the rune groups that ended up in the center position
        FlashTargetRuneGroups(targetIndices);

        // Signal completion if barrier token was provided
        if (completionToken.HasValue && _interactionBarrier != null)
        {
            _interactionBarrier.Signal(new BarrierKey(completionToken.Value));
        }
    }

    private IEnumerator SpinContainerToTarget(RectTransform container, Vector3 startingPosition, float delay, int targetIndex)
    {
        yield return new WaitForSeconds(delay);
        
        float containerHeight = GROUP_SPACING * TOTAL_GROUPS_PER_CONTAINER;
        float resetYPosition = startingPosition.y - containerHeight; // When to reset container to top
        
        // 1. WINDUP: Move container up slightly for dramatic effect
        Vector3 windupPos = container.anchoredPosition;
        windupPos.y += _windupDistance;
        yield return container.DOLocalMoveY(windupPos.y, _windupDuration).SetEase(Ease.OutBack).WaitForCompletion();
        
        // 2. SIMULATE SPINNING: Continuous downward movement with resets for spin duration
        float spinStartTime = Time.time;
        while (Time.time - spinStartTime < _spinDuration)
        {
            // Move container down
            Vector3 currentPos = container.anchoredPosition;
            currentPos.y -= _spinSpeed * Time.deltaTime;
            
            // If container went too far down, reset to top
            if (currentPos.y <= resetYPosition)
            {
                currentPos.y += containerHeight;
            }
            
            container.anchoredPosition = currentPos;
            yield return null;
        }
        
        // 3. RESET TO STARTING POSITION
        container.anchoredPosition = startingPosition;
        
        // 4. MOVE TO TARGET POSITION with overshoot
        float targetOffset = (targetIndex - 1) * GROUP_SPACING;
        Vector3 targetPosWithOvershoot = startingPosition;
        targetPosWithOvershoot.y -= targetOffset + _overshootDistance;
        
        yield return container.DOLocalMoveY(targetPosWithOvershoot.y, 0.3f).SetEase(_spinEase).WaitForCompletion();
        
        // 5. SETTLE: Bounce back to correct position without overshoot
        Vector3 finalPos = startingPosition;
        finalPos.y -= targetOffset;
        
        yield return container.DOLocalMoveY(finalPos.y, _settleDuration).SetEase(Ease.OutBounce).WaitForCompletion();
    }

    private void FlashTargetRuneGroups(int[] indices)
    {
        var targetIndices = indices.Select(i => TOTAL_GROUPS_PER_CONTAINER - 1 - i).ToArray();
        Debug.Log($"[RuneSlotMachine] FlashTargetRuneGroups called with indices: [{string.Join(", ", targetIndices)}]");

        // Collect all flashing rune types
        var flashingRunes = new List<RuneType>();

        // Get the rune data to debug what should be showing
        var allTumblerFaces = GetCurrentTumblerFaces();

        if (targetIndices.Length >= 1 && allTumblerFaces != null && allTumblerFaces.Length > 0)
        {
            var selectableGroups = GetSelectableGroups(0);
            var targetIndex = targetIndices[0];

            if (targetIndex >= 0 && targetIndex < selectableGroups.Count && targetIndex < allTumblerFaces[0].Count)
            {
                var targetFace = allTumblerFaces[0][targetIndex];
                var runeTypes = targetFace.GetRunesForDisplay();
                Debug.Log($"[RuneSlotMachine] Container 0, Index {targetIndex}: Expected runes [{string.Join(", ", runeTypes)}]");
                flashingRunes.AddRange(runeTypes);
                selectableGroups[targetIndex].FlashGlow();
            }
        }

        if (targetIndices.Length >= 2 && allTumblerFaces != null && allTumblerFaces.Length > 1)
        {
            var selectableGroups = GetSelectableGroups(1);
            var targetIndex = targetIndices[1];

            if (targetIndex >= 0 && targetIndex < selectableGroups.Count && targetIndex < allTumblerFaces[1].Count)
            {
                var targetFace = allTumblerFaces[1][targetIndex];
                var runeTypes = targetFace.GetRunesForDisplay();
                Debug.Log($"[RuneSlotMachine] Container 1, Index {targetIndex}: Expected runes [{string.Join(", ", runeTypes)}]");
                flashingRunes.AddRange(runeTypes);
                selectableGroups[targetIndex].FlashGlow();
            }
        }

        if (targetIndices.Length >= 3 && allTumblerFaces != null && allTumblerFaces.Length > 2)
        {
            var selectableGroups = GetSelectableGroups(2);
            var targetIndex = targetIndices[2];

            if (targetIndex >= 0 && targetIndex < selectableGroups.Count && targetIndex < allTumblerFaces[2].Count)
            {
                var targetFace = allTumblerFaces[2][targetIndex];
                var runeTypes = targetFace.GetRunesForDisplay();
                Debug.Log($"[RuneSlotMachine] Container 2, Index {targetIndex}: Expected runes [{string.Join(", ", runeTypes)}]");
                flashingRunes.AddRange(runeTypes);
                selectableGroups[targetIndex].FlashGlow();
            }
        }

        // Publish the rune flash event with all collected rune types
        if (flashingRunes.Count > 0)
        {
            _eventBus.Publish(new RuneFlashEvent(flashingRunes));
        }
    }

    private List<RuneFace>[] GetCurrentTumblerFaces()
    {
        return _currentRuneSlotMachine?.GetAllTumblerFaces();
    }

    public void PopulateWithRuneData(RuneSlotMachineEntity runeSlotMachine)
    {
        if (runeSlotMachine == null) return;

        // Store reference for debugging purposes
        _currentRuneSlotMachine = runeSlotMachine;

        var tumblerFacesData = runeSlotMachine.GetAllTumblerFaces();
        
        // Update first tumbler faces (including buffers)
        if (tumblerFacesData.Length > 0)
        {
            PopulateAllGroupsInContainer(_firstContainerGroups, tumblerFacesData[0]);
        }
        
        // Update second tumbler faces (including buffers)
        if (tumblerFacesData.Length > 1)
        {
            PopulateAllGroupsInContainer(_secondContainerGroups, tumblerFacesData[1]);
        }
        
        // Update third tumbler faces (including buffers)
        if (tumblerFacesData.Length > 2)
        {
            PopulateAllGroupsInContainer(_thirdContainerGroups, tumblerFacesData[2]);
        }
    }
    
    private void PopulateAllGroupsInContainer(List<RuneIconGroupUI> allGroups, List<RuneFace> faces)
    {
        if (faces.Count == 0) return;
        
        for (int i = 0; i < allGroups.Count; i++)
        {
            // Calculate which face this group should show (with wraparound for buffers)
            int faceIndex;
            
            if (i < BUFFER_ICONS_COUNT)
            {
                // Buffer icons at the start should show the last faces
                faceIndex = faces.Count - (BUFFER_ICONS_COUNT - i);
            }
            else if (i >= BUFFER_ICONS_COUNT + faces.Count)
            {
                // Buffer icons at the end should show the first faces
                faceIndex = (i - BUFFER_ICONS_COUNT) % faces.Count;
            }
            else
            {
                // Regular icons
                faceIndex = i - BUFFER_ICONS_COUNT;
            }
            
            faceIndex = ((faceIndex % faces.Count) + faces.Count) % faces.Count; // Ensure positive modulo
            allGroups[i].UpdateSprite(faces[faceIndex].GetRunesForDisplay());
        }
    }
    
    private void UpdateTumblerFaces(List<RuneIconGroupUI> tumblerGroups, List<RuneFace> faces)
    {
        for (int i = 0; i < tumblerGroups.Count && i < faces.Count; i++)
        {
            tumblerGroups[i].UpdateSprite(faces[i].GetRunesForDisplay());
        }
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
