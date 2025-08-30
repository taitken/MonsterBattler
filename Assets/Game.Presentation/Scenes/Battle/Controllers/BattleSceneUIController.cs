using System;
using System.Threading.Tasks;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging.Events.Rewards;
using Game.Core;
using Game.Domain.Enums;
using Game.Domain.Structs;
using Game.Presentation.Scenes.Battle.Controllers;
using UnityEngine;

public class BattleSceneUIController : MonoBehaviour
{
    [SerializeField] private RewardsWindow _rewardsWindow;
    [SerializeField] private CardSelectWindow _cardSelectWindow;
    [SerializeField] private RuneSlotMachineUI _slotMachine;
    private IEventBus _eventBus;
    private IDisposable _battleEventEnded;
    private IDisposable _battleStartedSubscription;
    private IDisposable _slotMachineSpinSubscription;
    private IDisposable _cardRewardSelectedSubscription;
    private BattleResult? _currentBattleResult;

    void Awake()
    {
        _eventBus = ServiceLocator.Get<IEventBus>();
        _battleEventEnded = _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        _battleStartedSubscription = _eventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);
        _slotMachineSpinSubscription = _eventBus.Subscribe<SlotMachineSpinEvent>(OnSlotMachineSpinRequested);
        _cardRewardSelectedSubscription = _eventBus.Subscribe<CardRewardSelectedEvent>(OnCardRewardSelected);
        
        var pauseUIController = gameObject.AddComponent<BattlePauseUIController>();
        pauseUIController.Initialize();
    }

    void OnDestroy()
    {
        _battleEventEnded.Dispose();
        _battleStartedSubscription.Dispose();
        _slotMachineSpinSubscription.Dispose();
        _cardRewardSelectedSubscription.Dispose();
    }

    void Start()
    {
        _rewardsWindow?.Hide();
        _cardSelectWindow?.Hide();
    }

    private async void OnBattleEnded(BattleEndedEvent @event)
    {
        if (@event.Result.Outcome == BattleOutcome.PlayerVictory)
        {
            _currentBattleResult = @event.Result;
            
            // Wait 1.5 seconds for battle animations to finish
            await Task.Delay(1500);
            
            _rewardsWindow?.ShowWithRewards(@event.Rewards, OnContinueClicked);
        }
    }
    
    private void OnContinueClicked()
    {
            Debug.Log("finished battle!");
        if (_currentBattleResult.HasValue)
        {
            Debug.Log("finished battle!");
            // Send the command to complete the battle flow
            _eventBus.Publish(new BattleFlowCompleteCommand(_currentBattleResult.Value));
        }
        
        _rewardsWindow?.Hide();
    }
    
    private void OnBattleStarted(BattleStartedEvent battleEvent)
    {
        // Pass the rune slot machine entity to the UI component for setup
        _slotMachine?.PopulateWithRuneData(battleEvent.RuneSlotMachine);
    }
    
    private void OnSlotMachineSpinRequested(SlotMachineSpinEvent spinEvent)
    {
        // Start the slot machine animation with the provided values and completion token
        _slotMachine?.StartSpin(spinEvent.WheelValues, spinEvent.CompletionToken);
    }
    
    private void OnCardRewardSelected(CardRewardSelectedEvent cardRewardEvent)
    {
        _cardSelectWindow?.Show();
    }
}
