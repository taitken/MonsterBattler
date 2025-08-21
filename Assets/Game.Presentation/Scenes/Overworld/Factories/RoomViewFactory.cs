using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.OverworldMap;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public class RoomViewFactory : IRoomViewFactory
    {
        private readonly GameObject roomPrefab;

        public RoomViewFactory(GameObject _roomPrefab)
        {
            roomPrefab = _roomPrefab ?? throw new System.ArgumentNullException(nameof(_roomPrefab), 
                "Room prefab cannot be null");
        }

        public RoomView Create(RoomEntity model, Vector3 spawnPoint)
        {
            if (model == null)
                throw new System.ArgumentNullException(nameof(model), "Cannot create RoomView with null model");
            
            if (roomPrefab == null)
                throw new System.InvalidOperationException("Room prefab is null. Factory was not properly initialized.");

            var obj = Object.Instantiate(roomPrefab, spawnPoint, Quaternion.identity);
            if (obj == null)
                throw new System.InvalidOperationException("Failed to instantiate room prefab");
                
            // Set scale to 30%
            obj.transform.localScale = Vector3.one * 0.45f;
                
            var roomView = obj.GetComponent<RoomView>();
            if (roomView == null)
            {
                Object.Destroy(obj); // Clean up the failed instantiation
                throw new System.InvalidOperationException(
                    $"RoomView component not found on prefab '{roomPrefab.name}'. " +
                    "Ensure the prefab has a RoomView component attached.");
            }
            
            roomView.Bind(model);
            return roomView;
        }
    }
}