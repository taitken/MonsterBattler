using UnityEngine;

namespace Game.Presentation.Controllers
{
    public class TiledBackground : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundTilePrefab;
        [SerializeField] private int tileGridSize = 3; // 3x3 grid of tiles around camera
        [SerializeField] private Vector2 tileSize = new Vector2(10f, 10f); // Size of each background tile
        [SerializeField] private Transform cameraTransform;
        
        private GameObject[,] _tiles;
        private Vector2 _currentCenterTile;
        private Camera _camera;

        void Start()
        {
            if (cameraTransform == null)
            {
                _camera = Camera.main;
                cameraTransform = _camera.transform;
            }
            
            if (backgroundTilePrefab == null)
            {
                Debug.LogError("Background tile prefab is not assigned!");
                return;
            }
            
            InitializeTileGrid();
            UpdateTilePositions();
        }

        void Update()
        {
            Vector2 cameraPosition = new Vector2(cameraTransform.position.x, cameraTransform.position.y);
            Vector2 newCenterTile = new Vector2(
                Mathf.FloorToInt(cameraPosition.x / tileSize.x),
                Mathf.FloorToInt(cameraPosition.y / tileSize.y)
            );

            // Only update tiles if camera moved to a different tile
            if (newCenterTile != _currentCenterTile)
            {
                _currentCenterTile = newCenterTile;
                UpdateTilePositions();
            }
        }

        private void InitializeTileGrid()
        {
            _tiles = new GameObject[tileGridSize, tileGridSize];
            
            for (int x = 0; x < tileGridSize; x++)
            {
                for (int y = 0; y < tileGridSize; y++)
                {
                    _tiles[x, y] = Instantiate(backgroundTilePrefab, transform);
                    _tiles[x, y].name = $"BackgroundTile_{x}_{y}";
                }
            }
            
            _currentCenterTile = Vector2.zero;
        }

        private void UpdateTilePositions()
        {
            int halfGrid = tileGridSize / 2;
            
            for (int x = 0; x < tileGridSize; x++)
            {
                for (int y = 0; y < tileGridSize; y++)
                {
                    if (_tiles[x, y] != null)
                    {
                        // Calculate tile position relative to camera
                        float tileX = (_currentCenterTile.x + (x - halfGrid)) * tileSize.x;
                        float tileY = (_currentCenterTile.y + (y - halfGrid)) * tileSize.y;
                        
                        Vector3 tilePosition = new Vector3(tileX, tileY, transform.position.z);
                        _tiles[x, y].transform.position = tilePosition;
                    }
                }
            }
        }

        // Method to set tile size automatically based on sprite bounds
        public void AutoCalculateTileSize()
        {
            if (backgroundTilePrefab != null)
            {
                SpriteRenderer spriteRenderer = backgroundTilePrefab.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    Bounds bounds = spriteRenderer.bounds;
                    tileSize = new Vector2(bounds.size.x, bounds.size.y);
                    Debug.Log($"Auto-calculated tile size: {tileSize}");
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw tile boundaries in scene view
            Gizmos.color = Color.yellow;
            Vector2 cameraPos = cameraTransform != null ? 
                new Vector2(cameraTransform.position.x, cameraTransform.position.y) : 
                Vector2.zero;
                
            int halfGrid = tileGridSize / 2;
            Vector2 centerTile = new Vector2(
                Mathf.FloorToInt(cameraPos.x / tileSize.x),
                Mathf.FloorToInt(cameraPos.y / tileSize.y)
            );
            
            for (int x = -halfGrid; x <= halfGrid; x++)
            {
                for (int y = -halfGrid; y <= halfGrid; y++)
                {
                    Vector3 tileCenter = new Vector3(
                        (centerTile.x + x) * tileSize.x + tileSize.x * 0.5f,
                        (centerTile.y + y) * tileSize.y + tileSize.y * 0.5f,
                        0
                    );
                    
                    Gizmos.DrawWireCube(tileCenter, new Vector3(tileSize.x, tileSize.y, 0));
                }
            }
        }
    }
}