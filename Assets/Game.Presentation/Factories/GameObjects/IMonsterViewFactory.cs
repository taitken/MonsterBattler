using Game.Domain.Entities;
using UnityEngine;
namespace Assets.Game.Presentation.GameObjects
{
    public interface IMonsterViewFactory
    {
        MonsterView Create(MonsterEntity model, Vector3 spawnPoint);
        MonsterView Create(MonsterEntity model, Transform spawnPoint);
    }
}