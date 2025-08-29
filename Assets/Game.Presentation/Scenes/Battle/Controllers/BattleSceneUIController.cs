using System;
using System.Threading.Tasks;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core;
using Game.Domain.Enums;
using Game.Domain.Structs;
using Game.Presentation.Scenes.Battle.Controllers;
using UnityEngine;

public class BattleSceneUIController : MonoBehaviour
{
    [SerializeField] private RewardsWindow _rewardsWindow;
    [SerializeField] private RuneSlotMachineUI _slotMachine;
    private IEventBus _eventBus;
    private IDisposable _battleEventEnded;
    private BattleResult? _currentBattleResult;

    void Awake()
    {
        _eventBus = ServiceLocator.Get<IEventBus>();
        _battleEventEnded = _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        
        var pauseUIController = gameObject.AddComponent<BattlePauseUIController>();
        pauseUIController.Initialize();
    }

    void OnDestroy()
    {
        _battleEventEnded.Dispose();
    }

    void Start()
    {
        _rewardsWindow?.Hide();
        _slotMachine.StartSpin(new int[] { 1, 6, 3 });
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
}
