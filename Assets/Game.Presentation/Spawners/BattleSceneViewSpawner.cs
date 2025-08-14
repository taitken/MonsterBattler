using System;
using System.Collections.Generic;
using Assets.Game.Presentation.GameObjects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events;
using Game.Application.Messaging.Events.Spawning;
using Game.Core;
using Game.Domain.Enums;
using Game.Presentation.GameObjects.Factories;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class MonsterViewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        private IEventBus _eventQueueService;
        private IDisposable _monsterEventSubscription;
        private IMonsterViewFactory _factory;
        private List<MonsterView> playerMonsters = new();
        private List<MonsterView> enemyMonsters = new();


        void Awake()
        {
            _eventQueueService = ServiceLocator.Get<IEventBus>();
            _factory = ServiceLocator.Get<IMonsterViewFactory>();
            _monsterEventSubscription = _eventQueueService.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
        }

        private void OnMonsterSpawned(MonsterSpawnedEvent evt)
        {
            Debug.Log($"Monster spawned: {evt.Monster.Type} on team {evt.Team}");
            var spawnPosition = DetermineMonsterSpawnPoint(evt);
            var monsterView = _factory.Create(evt.Monster, evt.Team, spawnPosition);
            var team = GetMonstersByTeam(evt.Team);
            team.Add(monsterView);
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
            _monsterEventSubscription?.Dispose();
        }
    }
}
