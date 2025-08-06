using Game.Domain.Entities;
using UnityEngine;
namespace Assets.Game.Presentation.GameObjects
{
    public interface IMonsterViewFactory
    {
        MonsterView Spawn(MonsterEntity model, Vector3 spawnPoint);
        MonsterView Spawn(MonsterEntity model, Transform spawnPoint);
    }
}