using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.OverworldMap;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public class RoomFactory : IRoomFactory
    {
        private readonly GameObject roomFactoryPrefab;
        private const float PLAYER_SCALE = 0.25f;
        private const float ENEMY_SCALE = 0.45f;

        public RoomFactory(GameObject _roomPrefab)
        {
            roomFactoryPrefab = _roomPrefab;
        }

        public RoomView Create(RoomEntity model, Vector3 spawnPoint)
        {
            var obj = Object.Instantiate(roomFactoryPrefab, spawnPoint, Quaternion.identity);
            var room = obj.GetComponent<RoomView>();
            room.Bind(model);
            return room;
        }
    }
}