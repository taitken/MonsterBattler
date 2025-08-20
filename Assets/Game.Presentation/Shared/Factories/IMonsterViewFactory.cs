using Game.Presentation.Shared.Views;
using Game.Domain.Entities;
using Game.Domain.Enums;
using UnityEngine;

namespace Game.Presentation.Shared.Factories
{
    public interface IMonsterViewFactory
    {
        MonsterView Create(MonsterEntity model, BattleTeam team, Vector3 spawnPoint);
    }
}