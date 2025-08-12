using UnityEngine;


namespace Game.Presentation.UI.BackgroundUi
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FullscreenBackground : MonoBehaviour
    {
        void Start()
        {
            var sr = GetComponent<SpriteRenderer>();
            var cam = Camera.main;

            // Get world dimensions of the camera
            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;

            // Get the sprite's original size in world units
            float spriteWidth = sr.sprite.bounds.size.x;
            float spriteHeight = sr.sprite.bounds.size.y;

            // Scale the sprite so it fills the camera view
            transform.localScale = new Vector3(
                worldWidth / spriteWidth,
                worldHeight / spriteHeight,
                1f
            );

            // Push it behind everything
            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
            sr.sortingOrder = -100;
        }
    }
}