using Game.Domain.Entities;
using Game.Domain.Enums;
using UnityEngine;

namespace Assets.Game.Presentation.GameObjects
{
    public class MonsterViewFactory : IMonsterViewFactory
    {
        private readonly GameObject monsterPrefab;
        private const float PLAYER_SCALE = 0.25f;
        private const float ENEMY_SCALE = 0.45f;

        public MonsterViewFactory(GameObject _monsterPrefab)
        {
            monsterPrefab = _monsterPrefab;
        }

        public MonsterView Create(MonsterEntity model, BattleTeam team, Vector3 spawnPoint)
        {
            var obj = Object.Instantiate(monsterPrefab, spawnPoint, Quaternion.identity);
            var monster = obj.GetComponent<MonsterView>();
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