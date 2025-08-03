using System.Collections.Generic;
using UnityEngine;

namespace UtilityClasses
{
    public class ObjectMovement
    {
        public Transform transform { get; set; }
        public IList<Vector3> movePath { get; set; }
        public float moveSpeed { get; set; }
        public bool updateSpriteDirection { get; set; }
        public Vector2 finalDirection { get; set; }
        public EventEmitter onMovementFinished { get; set; }
        public bool showPathingLine { get; set; }

        public ObjectMovement(Transform _transform, IList<Vector3> _movePath, float _moveSpeed, bool _updateSpriteDirection, bool _showPathingLine)
        {
            this.transform = _transform;
            this.movePath = _movePath;
            this.moveSpeed = _moveSpeed;
            this.updateSpriteDirection = _updateSpriteDirection;
            this.showPathingLine = _showPathingLine;
            this.onMovementFinished = new EventEmitter();
        }

        public void CancelMovement()
        {
            this.movePath = null;
        }
    }
}