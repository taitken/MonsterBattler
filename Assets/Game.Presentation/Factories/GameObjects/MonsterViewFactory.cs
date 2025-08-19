using Assets.Game.Presentation.GameObjects;
using Game.Domain.Entities;
using Game.Domain.Enums;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public class MonsterViewFactory : IMonsterViewFactory
    {
        private readonly GameObject monsterPrefab;
        private const float PLAYER_SCALE = 0.25f;
        private const float ENEMY_SCALE = 0.35f;

        public MonsterViewFactory(GameObject _monsterPrefab)
        {
            monsterPrefab = _monsterPrefab ?? throw new System.ArgumentNullException(nameof(_monsterPrefab), 
                "Monster prefab cannot be null");
        }

        public MonsterView Create(MonsterEntity model, BattleTeam team, Vector3 spawnPoint)
        {
            if (model == null)
                throw new System.ArgumentNullException(nameof(model), "Cannot create MonsterView with null model");
            
            if (monsterPrefab == null)
                throw new System.InvalidOperationException("Monster prefab is null. Factory was not properly initialized.");

            var obj = Object.Instantiate(monsterPrefab, spawnPoint, Quaternion.identity);
            if (obj == null)
                throw new System.InvalidOperationException("Failed to instantiate monster prefab");
                
            var monster = obj.GetComponent<MonsterView>();
            if (monster == null)
            {
                Object.Destroy(obj); // Clean up the failed instantiation
                throw new System.InvalidOperationException(
                    $"MonsterView component not found on prefab '{monsterPrefab.name}'. " +
                    "Ensure the prefab has a MonsterView component attached.");
            }
            
            monster.transform.localScale = GetModelScale(team);
            monster.Bind(model);
            return monster;
        }

        private Vector3 GetModelScale(BattleTeam team)
        {
            return team == BattleTeam.Player ? new Vector3(PLAYER_SCALE, PLAYER_SCALE, PLAYER_SCALE) : new Vector3(ENEMY_SCALE, ENEMY_SCALE, ENEMY_SCALE);
        }
    }
}