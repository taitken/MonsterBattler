using UnityEngine;

namespace Game.Presentation.UI
{
    /// <summary>
    /// Scales the background sprite to fit the camera view.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundUI : MonoBehaviour
    {
        private SpriteRenderer _sr;
        void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            ScaleToCamera(Camera.main);
        }

        public void SetImage(Sprite image)
        {
            _sr.sprite = image;
            ScaleToCamera(Camera.main);
        }

        public void ScaleToCamera(Camera cam)
        {
            if (_sr.sprite == null || cam == null) return;

            // Get sprite size in world units
            float spriteWidth = _sr.sprite.bounds.size.x;
            float spriteHeight = _sr.sprite.bounds.size.y;

            // Get camera size
            float worldScreenHeight = cam.orthographicSize * 2f;
            float worldScreenWidth = worldScreenHeight * cam.aspect;

            // Calculate scale multiplier
            Vector3 newScale = transform.localScale;
            newScale.x = worldScreenWidth / spriteWidth;
            newScale.y = worldScreenHeight / spriteHeight;

            transform.localScale = newScale;
        }
    }
}