using Game.Domain.Entities;
using UnityEngine;
namespace Assets.Game.Presentation.GameObjects
{
    public class MonsterViewFactory : IMonsterViewFactory
    {
        private readonly GameObject monsterPrefab;

        public MonsterViewFactory(GameObject _monsterPrefab)
        {
            monsterPrefab = _monsterPrefab;
        }

        public MonsterView Spawn(MonsterEntity model, Vector3 spawnPoint)
        {
            var obj = Object.Instantiate(monsterPrefab, spawnPoint, Quaternion.identity);
            var monster = obj.GetComponent<MonsterView>();
            monster.Bind(model);
            return monster;
        }

        public MonsterView Spawn(MonsterEntity model, Transform spawnPoint)
        {
            var obj = Object.Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
            var monster = obj.GetComponent<MonsterView>();
            monster.Bind(model);
            return monster;
        }
    }
}