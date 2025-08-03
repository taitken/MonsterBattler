using System;
using System.Collections.Generic;
using Building.Models;
using Environment;
using Environment.Models;
using GameControllers.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UtilityClasses;

namespace UnityEngine
{
    public abstract class MonoBehaviourLayer : MonoBehaviour2
    {
        public const int MAP_WIDTH = 100;
        public const int MAP_HEIGHT = 100;
        protected Tilemap tilemap;
        LayerCollider layerCollider;
        protected void InitiliseMonoLayer(LayerCollider.Factory _layerColliderFactory, Vector2 _size, string _layer)
        {
            this.tilemap = this.GetComponent<Tilemap>();
            IList<Action> callbacks = new List<Action>() { this.OnClickedByUser, this.OnMouseEnter, this.OnMouseExit };
            IList<Action<DragEventModel>> dragCallbacks = new List<Action<DragEventModel>>() { this.OnDrag, this.OnDragEnd };
            this.layerCollider = _layerColliderFactory.Create(_size, _layer, callbacks, dragCallbacks);
            this.layerCollider.gameObject.transform.SetParent(this.transform);
        }

        protected Vector3 GetLocalPositionOfCellAtMouse()
        {
            Vector3 cameraPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            return this.tilemap.CellToLocal(this.tilemap.LocalToCell(new Vector3(cameraPoint.x + 0.05f, cameraPoint.y + 0.05f)));
        }

        protected Vector3Int GetCellCoorAtMouse()
        {
            Vector3 cameraPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            return this.tilemap.LocalToCell(new Vector3(cameraPoint.x + 0.05f, cameraPoint.y + 0.05f));
        }


        protected Vector3Int MoveObjectOffInvalidPosition(IList<MonoBaseObject> objectsToMove, Vector3Int positionToMoveOff, PathFinderMap pfMap)
        {
            IList<MonoBaseObject> objects = objectsToMove.Filter(itemObj => { return itemObj.GetBaseObjectModel().position == positionToMoveOff; });
            Vector3Int newPosition = default(Vector3Int);
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x - 1, positionToMoveOff.y, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x + 1, positionToMoveOff.y, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x, positionToMoveOff.y - 1, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x, positionToMoveOff.y + 1, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x - 1, positionToMoveOff.y - 1, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x + 1, positionToMoveOff.y + 1, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x + 1, positionToMoveOff.y - 1, pfMap); }
            if (newPosition == default(Vector3Int)) { newPosition = this.CheckAndGetPosition(positionToMoveOff.x - 1, positionToMoveOff.y + 1, pfMap); }
            if (newPosition != default(Vector3Int))
            {
                objects.ForEach(objectToMove =>
                {
                    this.SetItemPosition(objectToMove, newPosition);
                });
                return newPosition;
            }
            return default(Vector3Int);
        }

        private Vector3Int CheckAndGetPosition(int x, int y, PathFinderMap pfMap)
        {
            if (this.CheckIfSpotFree(x, y, pfMap))
            {
                return new Vector3Int(x, y);
            }
            return default(Vector3Int);
        }

        private bool CheckIfSpotFree(int x, int y, PathFinderMap pfMap)
        {
            return !pfMap.mapitems[x, y].impassable;
        }

        private void SetItemPosition(MonoBaseObject baseObject, Vector3Int pos)
        {
            baseObject.transform.position = this.tilemap.CellToLocal(pos);
            baseObject.GetBaseObjectModel().position = pos;
        }
    }
}