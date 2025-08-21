using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging.Events.Spawning;
using Game.Core;
using Game.Presentation.Scenes.Battle.Controllers;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Scenes.Battle.Spawners;
using Game.Presentation.Shared.Enums;
using Game.Presentation.Core.Interfaces;
using UnityEngine;
using Game.Presentation.Shared.Views;
using Game.Presentation.Spawners;
using Game.Presentation.Scenes.Battle.Services;

namespace Game.Presentation.Scenes.Battle.Coordinators
{
    public class BattleSceneCoordinator : MonoBehaviour
    {
        [SerializeField] private MonsterViewSpawner monsterSpawner;
        [SerializeField] private CardAnimationController cardAnimationController;
        [SerializeField] private CardViewManager cardViewManager;
        
        private IEventBus _eventBus;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;
        private IMonsterViewFactory _monsterFactory;
        private ICardViewFactory _cardFactory;
        
        private IDisposable _monsterEventSubscription;
        private IDisposable _battleEndedSubscription;
        private IDisposable _cardPlayedSubscription;
        private IDisposable _cardsDrawnSubscription;
        private IDisposable _monsterFaintedSubscription;

        void Awake()
        {
            // Get dependencies
            _eventBus = ServiceLocator.Get<IEventBus>();
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _monsterFactory = ServiceLocator.Get<IMonsterViewFactory>();
            _cardFactory = ServiceLocator.Get<ICardViewFactory>();
            
            // Initialize components
            monsterSpawner.Initialize(_waitBarrier, _viewRegistry, _monsterFactory);
            cardAnimationController.Initialize(_waitBarrier, _viewRegistry);
            cardViewManager.Initialize(_cardFactory);
            
            // Subscribe to events
            _monsterEventSubscription = _eventBus.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
            _battleEndedSubscription = _eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
            _cardPlayedSubscription = _eventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            _cardsDrawnSubscription = _eventBus.Subscribe<CardsDrawnEvent>(OnCardsDrawn);
            _monsterFaintedSubscription = _eventBus.Subscribe<MonsterFaintedEvent>(OnMonsterFainted);
        }

        private void OnMonsterSpawned(MonsterSpawnedEvent evt)
        {
            monsterSpawner.SpawnMonster(evt);
        }

        private void OnBattleEnded(BattleEndedEvent evt)
        {
            monsterSpawner.CleanupAllMonsters();
            cardViewManager.ClearAllDrawnCards();
        }

        private void OnMonsterFainted(MonsterFaintedEvent evt)
        {
            // Destroy the card associated with the dead monster
            cardViewManager.DestroyCardForMonster(evt.Monster);
        }

        private void OnCardsDrawn(CardsDrawnEvent evt)
        {
            // Create card views through the manager
            var cardViews = cardViewManager.CreateCardsForDraw(evt.DrawnCards, _viewRegistry);
            
            // Start animation through the controller
            StartCoroutine(cardAnimationController.AnimateCardsDrawn(evt.DrawnCards, cardViews, evt.CompletionToken));
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // Try to get the pre-drawn card
            var cardView = cardViewManager.TakeDrawnCard(evt.Caster);
            
            if (cardView == null)
            {
                Debug.LogWarning($"Could not find pre-drawn card for {evt.Caster.MonsterName}, creating new one");
                cardView = cardViewManager.CreateCardForAction(evt.Caster, evt.Card, _viewRegistry);
                
                if (cardView == null)
                {
                    Debug.LogError($"Failed to create fallback card for {evt.Caster.MonsterName}");
                    return;
                }
            }
            
            // Determine animation type and get target position if needed
            var animationType = CardAnimationTypeResolver.GetAnimationType(evt.Card);
            Vector3? targetPosition = null;
            
            if (animationType == CardAnimationType.Attack && evt.PrimaryTarget != null)
            {
                _viewRegistry.TryGet(evt.PrimaryTarget.Id, out MonsterView targetView);
                if (targetView != null)
                {
                    targetPosition = targetView.transform.position;
                }
            }
            
            // Start action animation through the controller
            StartCoroutine(cardAnimationController.AnimateCardAction(cardView, evt.AnimationToken, animationType, targetPosition));
        }

        void OnDestroy()
        {
            _monsterEventSubscription?.Dispose();
            _battleEndedSubscription?.Dispose();
            _cardPlayedSubscription?.Dispose();
            _cardsDrawnSubscription?.Dispose();
            _monsterFaintedSubscription?.Dispose();
        }
    }
}