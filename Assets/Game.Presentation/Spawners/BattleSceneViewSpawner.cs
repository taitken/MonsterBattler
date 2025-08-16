using System;
using System.Collections.Generic;
using Assets.Game.Presentation.GameObjects;
using Game.Application.DTOs;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging.Events.Spawning;
using Game.Core;
using Game.Domain.Enums;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Services;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class MonsterViewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        private IEventBus _eventQueueService;
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;
        private IDisposable _monsterEventSubscription;
        private IDisposable _battleStartedSubscription;
        private IMonsterViewFactory _factory;
        private List<MonsterView> playerMonsters = new();
        private List<MonsterView> enemyMonsters = new();
        private List<MonsterView> allMonsterViews = new(); // Track all views for cleanup
        private int _spawnedMonsterCount;


        void Awake()
        {
            _eventQueueService = ServiceLocator.Get<IEventBus>();
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _factory = ServiceLocator.Get<IMonsterViewFactory>();
            _monsterEventSubscription = _eventQueueService.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
            _battleStartedSubscription = _eventQueueService.Subscribe<BattleEndedEvent>(OnBattleEnded);
        }

        private void OnBattleEnded(BattleEndedEvent evt)
        {
            _spawnedMonsterCount = 0;
            CleanupMonsterViews();
        }

        private void CleanupMonsterViews()
        {
            // Unregister all views from the registry
            foreach (var view in allMonsterViews)
            {
                if (view != null && view.model != null)
                {
                    _viewRegistry.Unregister(view.model.Id);
                    Debug.Log($"Unregistered {view.model.MonsterName} (ID: {view.model.Id}) from ViewRegistry");
                }
            }
            
            // Clear all lists
            allMonsterViews.Clear();
            playerMonsters.Clear();
            enemyMonsters.Clear();
        }

        private void OnMonsterSpawned(MonsterSpawnedEvent evt)
        {
            // Handle barrier synchronization first (count all spawn events, even for dead monsters)
            if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue)
            {
                _spawnedMonsterCount++;
                Debug.Log($"Processed {_spawnedMonsterCount}/{ evt.ExpectedTotalCount} monster spawn events");
            }

            // Don't create views for dead monsters
            if (evt.Monster.IsDead)
            {
                Debug.Log($"Skipping view creation for dead monster: {evt.Monster.MonsterName} (HP: {evt.Monster.CurrentHP})");
                
                // Check if all spawn events are processed (including skipped ones)
                if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue && 
                    _spawnedMonsterCount >= evt.ExpectedTotalCount)
                {
                    Debug.Log("All monster spawn events processed, signaling completion");
                    _waitBarrier.Signal(new BarrierKey((BarrierToken)evt.SpawnCompletionToken));
                }
                return;
            }

            Debug.Log($"Creating view for monster: {evt.Monster.Type} on team {evt.Team}");
            var spawnPosition = DetermineMonsterSpawnPoint(evt);
            var monsterView = _factory.Create(evt.Monster, evt.Team, spawnPosition);
            var team = GetMonstersByTeam(evt.Team);
            team.Add(monsterView);
            allMonsterViews.Add(monsterView);

            // Register the view immediately in the ViewRegistry
            _viewRegistry.Register(evt.Monster.Id, monsterView);
            Debug.Log($"Registered {evt.Monster.MonsterName} (ID: {evt.Monster.Id}) with ViewRegistry in spawner");

            // Check if all spawn events are processed
            if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue && 
                _spawnedMonsterCount >= evt.ExpectedTotalCount)
            {
                Debug.Log("All monster spawn events processed, signaling completion");
                _waitBarrier.Signal(new BarrierKey((BarrierToken)evt.SpawnCompletionToken));
            }
        }

        private Vector3 DetermineMonsterSpawnPoint(MonsterSpawnedEvent evt)
        {
            var defaultSpawnPosition = GetDefaultSpawnPositionByTeam(evt.Team);
            var existingMonsters = GetMonstersByTeam(evt.Team);
            switch (existingMonsters.Count)
            {
                case 0:
                    return defaultSpawnPosition;
                case 1:
                    return defaultSpawnPosition + new Vector3(-2.5f, -1f, 0f);
                case 2:
                    return defaultSpawnPosition + new Vector3(2.0f, -1f, 0f);
                default:
                    return defaultSpawnPosition;
            }
        }

        private List<MonsterView> GetMonstersByTeam(BattleTeam team)
        {
            return team == BattleTeam.Player ? playerMonsters : enemyMonsters;
        }

        private Vector3 GetDefaultSpawnPositionByTeam(BattleTeam team)
        {
            return team == BattleTeam.Player ? playerSpawn.position : enemySpawn.position;
        }

        void OnDestroy()
        {
            CleanupMonsterViews();
            _monsterEventSubscription?.Dispose();
            _battleStartedSubscription?.Dispose();
        }
    }
}
