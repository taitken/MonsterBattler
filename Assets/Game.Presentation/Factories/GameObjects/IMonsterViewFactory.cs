using Game.Domain.Entities;
using Game.Domain.Enums;
using UnityEngine;
namespace Assets.Game.Presentation.GameObjects
{
    public interface IMonsterViewFactory
    {
        MonsterView Create(MonsterEntity model, BattleTeam team, Vector3 spawnPoint);
    }
}