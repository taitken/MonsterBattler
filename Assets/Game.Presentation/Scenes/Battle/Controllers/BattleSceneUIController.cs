using System;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Repositories;
using Game.Core;
using Game.Domain.Enums;
using Game.Domain.Structs;
using Game.Presentation.Scenes.Battle.Controllers;
using UnityEngine;

public class BattleSceneUIController : MonoBehaviour
{
    [SerializeField] private RewardsWindow _rewardsWindow;
    
    private IEventBus _eventBus;
    private IBattleHistoryRepository _battleHistory;
    private IDisposable _battleEventEnded;
    private BattleResult? _currentBattleResult;

    void Awake()
    {
        _eventBus = ServiceLocator.Get<IEventBus>();
        _battleHistory = ServiceLocator.Get<IBattleHistoryRepository>();
        _battleEventEnded = _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        
        var pauseUIController = gameObject.AddComponent<BattlePauseUIController>();
        pauseUIController.Initialize(_rewardsWindow);
    }

    void OnDestroy()
    {
        _battleEventEnded.Dispose();
    }

    void Start()
    {
        _rewardsWindow?.Hide();
    }

    private void OnBattleEnded(BattleEndedEvent @event)
    {
        if (@event.Result.Outcome == BattleOutcome.PlayerVictory)
        {
            _currentBattleResult = @event.Result;
            _rewardsWindow?.Show(@event.Result);
            _rewardsWindow?.SetupContinueButton(OnContinueClicked);
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
