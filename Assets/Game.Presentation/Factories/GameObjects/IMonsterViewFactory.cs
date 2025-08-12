using Assets.Game.Presentation.GameObjects;
using Game.Domain.Entities;
using Game.Domain.Enums;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public interface IMonsterViewFactory
    {
        MonsterView Create(MonsterEntity model, BattleTeam team, Vector3 spawnPoint);
    }
}