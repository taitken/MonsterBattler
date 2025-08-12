using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.OverworldMap;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public interface IRoomFactory
    {
        RoomView Create(RoomEntity model, Vector3 spawnPoint);
    }
}