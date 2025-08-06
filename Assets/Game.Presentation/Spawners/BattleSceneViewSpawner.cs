using System.Collections.Generic;
using Assets.Game.Presentation.GameObjects;
using Game.Application.Events.Battle;
using Game.Core;
using Game.Core.Events;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class MonsterViewSpawner : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private Transform enemySpawn;
        private IEventQueueService _eventQueueService;
        private IMonsterViewFactory _factory;
        private List<MonsterView> playerMonsters = new();
        private List<MonsterView> enemyMonsters = new();


        void Awake()
        {
            _eventQueueService = ServiceLocator.Get<IEventQueueService>();
            _factory = ServiceLocator.Get<IMonsterViewFactory>();
            _eventQueueService.Subscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
        }

        private void OnMonsterSpawned(MonsterSpawnedEvent evt)
        {
            var spawnPosition = DetermineMonsterSpawnPoint(evt);
            var monsterView = _factory.Create(evt.Monster, spawnPosition);
            var team = GetMonstersByTeam(evt.Team);
            team.Add(monsterView);
            monsterView.Bind(evt.Monster);
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
            _eventQueueService.Unsubscribe<MonsterSpawnedEvent>(OnMonsterSpawned);
        }
    }
}
