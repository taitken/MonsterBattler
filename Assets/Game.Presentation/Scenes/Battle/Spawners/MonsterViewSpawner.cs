using System.Collections.Generic;
using Game.Application.DTOs;
using Game.Application.Interfaces;
using Game.Application.Messaging.Events.Spawning;
using Game.Domain.Enums;
using Game.Presentation.Core.Interfaces;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Shared.Views;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class MonsterViewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        
        private IInteractionBarrier _waitBarrier;
        private IViewRegistryService _viewRegistry;
        private IMonsterViewFactory _factory;
        
        private List<MonsterView> playerMonsters = new();
        private List<MonsterView> enemyMonsters = new();
        private List<MonsterView> allMonsterViews = new();
        private int _spawnedMonsterCount;

        public void Initialize(IInteractionBarrier waitBarrier, IViewRegistryService viewRegistry, IMonsterViewFactory factory)
        {
            _waitBarrier = waitBarrier;
            _viewRegistry = viewRegistry;
            _factory = factory;
        }

        public void SpawnMonster(MonsterSpawnedEvent evt)
        {
            // Handle barrier synchronization first (count all spawn events, even for dead monsters)
            if (evt.SpawnCompletionToken.HasValue && evt.ExpectedTotalCount.HasValue)
            {
                _spawnedMonsterCount++;
                Debug.Log($"Processed {_spawnedMonsterCount}/{evt.ExpectedTotalCount} monster spawn events");
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

        public void CleanupAllMonsters()
        {
            _spawnedMonsterCount = 0;
            
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

        private Vector3 DetermineMonsterSpawnPoint(MonsterSpawnedEvent evt)
        {
            var defaultSpawnPosition = GetDefaultSpawnPositionByTeam(evt.Team);
            var existingMonsters = GetMonstersByTeam(evt.Team);
            switch (existingMonsters.Count)
            {
                case 0:
                    return defaultSpawnPosition;
                case 1:
                    return defaultSpawnPosition + new Vector3(-2.5f, -.5f, 0f);
                case 2:
                    return defaultSpawnPosition + new Vector3(2.4f, -.9f, 0f);
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
    }
}