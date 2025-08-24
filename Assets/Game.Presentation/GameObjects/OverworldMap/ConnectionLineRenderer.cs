using UnityEngine;
using System.Collections.Generic;

namespace Game.Presentation.GameObjects.OverworldMap
{
    public class ConnectionLineRenderer : MonoBehaviour
    {
        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Color lineColor = new Color(0.15f, 0.15f, 0.15f, .7f);
        [SerializeField] private int sortingOrder = 5; // Behind rooms but in front of background
        [SerializeField] private float dashLength = 0.3f;
        [SerializeField] private float gapLength = 0.25f;
        [SerializeField] private float roomMargin = 0.5f; // Distance from room center to start/end line

        private List<LineRenderer> _dashLines = new List<LineRenderer>();

        void Awake()
        {
            // Initialization will happen in SetConnection
        }

        private LineRenderer CreateDashLineRenderer()
        {
            var dashObject = new GameObject("Dash");
            dashObject.transform.SetParent(transform);
            
            var lineRenderer = dashObject.AddComponent<LineRenderer>();

            // Use provided material or create a default one
            if (lineMaterial != null)
            {
                lineRenderer.material = lineMaterial;
            }
            else
            {
                // Create a default material using Unity's built-in sprite default
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.sortingOrder = sortingOrder;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            
            return lineRenderer;
        }

        public void SetConnection(Vector3 startPosition, Vector3 endPosition)
        {
            // Clear existing dash lines
            ClearDashLines();
            
            // Calculate direction and total distance
            Vector3 direction = (endPosition - startPosition).normalized;
            float totalDistance = Vector3.Distance(startPosition, endPosition);
            
            // Apply room margins - move start and end points inward
            Vector3 adjustedStartPosition = startPosition + direction * roomMargin;
            Vector3 adjustedEndPosition = endPosition - direction * roomMargin;
            float adjustedDistance = Vector3.Distance(adjustedStartPosition, adjustedEndPosition);
            
            // Don't draw if the line would be too short after applying margins
            if (adjustedDistance <= 0.1f)
                return;
            
            // Calculate how many dashes we can fit
            float dashPattern = dashLength + gapLength;
            int dashCount = Mathf.FloorToInt(adjustedDistance / dashPattern);
            
            // Create dash lines using adjusted positions
            Vector3 currentPosition = adjustedStartPosition;
            for (int i = 0; i < dashCount; i++)
            {
                Vector3 dashStart = currentPosition;
                Vector3 dashEnd = currentPosition + direction * dashLength;
                
                var dashLine = CreateDashLineRenderer();
                dashLine.SetPosition(0, dashStart);
                dashLine.SetPosition(1, dashEnd);
                
                _dashLines.Add(dashLine);
                
                // Move to next dash position (including gap)
                currentPosition += direction * dashPattern;
            }
            
            // Handle remaining distance with a final dash if there's enough space
            float remainingDistance = adjustedDistance - (dashCount * dashPattern);
            if (remainingDistance > dashLength * 0.5f) // Only draw if at least half a dash length
            {
                Vector3 finalDashStart = currentPosition;
                Vector3 finalDashEnd = Vector3.Lerp(finalDashStart, adjustedEndPosition, Mathf.Min(1f, dashLength / remainingDistance));
                
                var finalDashLine = CreateDashLineRenderer();
                finalDashLine.SetPosition(0, finalDashStart);
                finalDashLine.SetPosition(1, finalDashEnd);
                
                _dashLines.Add(finalDashLine);
            }
        }
        
        private void ClearDashLines()
        {
            foreach (var dashLine in _dashLines)
            {
                if (dashLine != null)
                {
                    DestroyImmediate(dashLine.gameObject);
                }
            }
            _dashLines.Clear();
        }

        public void SetLineColor(Color color)
        {
            lineColor = color;
            foreach (var dashLine in _dashLines)
            {
                if (dashLine != null)
                {
                    dashLine.startColor = lineColor;
                    dashLine.endColor = lineColor;
                }
            }
        }

        public void SetLineWidth(float width)
        {
            lineWidth = width;
            foreach (var dashLine in _dashLines)
            {
                if (dashLine != null)
                {
                    dashLine.startWidth = width;
                    dashLine.endWidth = width;
                }
            }
        }
        
        void OnDestroy()
        {
            ClearDashLines();
        }
    }
}