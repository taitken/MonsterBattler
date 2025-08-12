using Game.Application.Messaging;
using Game.Core;
using Game.Presentation.Messaging;
using UnityEngine;

namespace Game.Presentation.UI.BackgroundUi
{
    /// <summary>
    /// Scales the background sprite to fit the camera view.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundUi : MonoBehaviour
    {
        void Start()
        {
            ScaleToCamera(Camera.main);
        }

        public void ScaleToCamera(Camera cam)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null || cam == null) return;

            // Get sprite size in world units
            float spriteWidth = sr.sprite.bounds.size.x;
            float spriteHeight = sr.sprite.bounds.size.y;

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