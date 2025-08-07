using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI.Combat
{
    [RequireComponent(typeof(RectTransform))]
    public class StretchableButtonFrame : MonoBehaviour
    {
        [Header("Sprites")]
        public Sprite cornerSprite;
        public Sprite horizontalSprite;
        public Sprite verticalSprite;

        [Header("Frame Settings")]
        public float cornerSize = 16f;
        public float borderThickness = 16f;
        [Range(1f, 3f)]
        public float borderScale = 1.0f;

        [Header("Target Dimensions")]
        public float buttonWidth = 200f;
        public float buttonHeight = 100f;

        private Image _backgroundImage;
        private RectTransform rectTransform;
        private float _lastWidth, _lastHeight;
        private bool needsRebuild = true;
        private Button _button;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(OnClickFeedback);
        }

        private void OnEnable()
        {
            RebuildIfNeeded();
        }

        [ContextMenu("Force Rebuild Frame")]
        public void RebuildNow()
        {
            Rebuild();
        }

        private void RebuildIfNeeded()
        {
            if (!needsRebuild) return;
            needsRebuild = false;
            Rebuild();
        }

        private void OnClickFeedback()
        {
            Debug.Log("Button clicked: " + gameObject.name);
            if (_backgroundImage == null) return;

            // Flash the background color briefly
            StopAllCoroutines();
            StartCoroutine(FlashColor(_backgroundImage, Color.white, 0.15f));
        }

        private IEnumerator FlashColor(Image img, Color flashColor, float duration)
        {
            Color originalColor = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(duration);
            img.color = originalColor;
        }

        private void Rebuild()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            ClearChildren();

            // Create or find the inner frame container
            var frameRoot = GetOrCreateFrameRoot();
            frameRoot.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            frameRoot.anchoredPosition = Vector2.zero;

            BuildFrame(frameRoot);
            SnapChildrenToPixels(frameRoot);
            AddBackground(frameRoot, new Color(1f, 0.6f, 0.3f)); // Light transparent white
        }

        private void AddDebugBackground(RectTransform parent, Color color)
        {
            GameObject go = new GameObject("DEBUG_Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = color;
        }

        private float GetHorizontalThickness()
        {
            if (cornerSprite == null || horizontalSprite == null)
                return cornerSize;

            float cornerPx = cornerSprite.rect.height;
            float barPx = horizontalSprite.rect.height;
            return cornerSize * (barPx / cornerPx);
        }

        private float GetVerticalThickness()
        {
            if (cornerSprite == null || verticalSprite == null)
                return cornerSize;

            float cornerPx = cornerSprite.rect.width;
            float barPx = verticalSprite.rect.width;
            return cornerSize * (barPx / cornerPx);
        }

        private void BuildFrame(RectTransform frameRoot)
        {
            float cornerAspectRatio = cornerSprite.rect.width / cornerSprite.rect.height;
            float w = buttonWidth;
            float h = buttonHeight;
            float hThickness = GetHorizontalThickness() * borderScale;
            float vThickness = GetVerticalThickness() * borderScale;
            Vector2 cornerSizeVec = new Vector2(
                cornerSize * cornerAspectRatio * borderScale,
                cornerSize * borderScale
            );
            Vector2 topOffset = new Vector2(0, -3f) * borderScale;
            Vector2 bottomOffset = new Vector2(0, 3f) * borderScale;


            // Horizontal edges
            CreateImage("Top", horizontalSprite, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(w - 2 * cornerSizeVec.x, hThickness), Vector3.zero, frameRoot, topOffset);
            CreateImage("Bottom", horizontalSprite, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(w - 2 * cornerSizeVec.x, hThickness), new Vector3(180, 0, 0), frameRoot, bottomOffset);

            // Vertical edges
            CreateImage("Left", verticalSprite, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(vThickness, h - 2 * cornerSizeVec.y), Vector3.zero, frameRoot);
            CreateImage("Right", verticalSprite, new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(vThickness, h - 2 * cornerSizeVec.y), new Vector3(0, 180, 0), frameRoot);

            // Corners
            CreateImage("TopLeft", cornerSprite, new Vector2(0, 1), new Vector2(0, 1), cornerSizeVec, new Vector3(0, 0, 0), frameRoot);
            CreateImage("TopRight", cornerSprite, new Vector2(1, 1), new Vector2(0, 1), cornerSizeVec, new Vector3(0, 180, 0), frameRoot);
            CreateImage("BottomLeft", cornerSprite, new Vector2(0, 0), new Vector2(0, 1), cornerSizeVec, new Vector3(180, 0, 0), frameRoot);
            CreateImage("BottomRight", cornerSprite, new Vector2(1, 0), new Vector2(0, 1), cornerSizeVec, new Vector3(180, 180, 0), frameRoot);
        }

        private RectTransform GetOrCreateFrameRoot()
        {
            var existing = transform.Find("FrameRoot");
            if (existing != null)
                DestroyImmediate(existing.gameObject);

            GameObject go = new GameObject("FrameRoot", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            return rt;
        }

        private void CreateImage(
            string name,
            Sprite sprite,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 size,
            Vector3 rotation,
            Transform parent,
            Vector2? offset = null)
        {
            if (sprite == null) return;

            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;

            Vector2 rawOffset = offset ?? Vector2.zero;
            rt.anchoredPosition = new Vector2(Mathf.Round(rawOffset.x), Mathf.Round(rawOffset.y));

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            go.transform.localEulerAngles = rotation;
        }

        private void SnapChildrenToPixels(Transform parent)
        {
            foreach (RectTransform rt in parent)
            {
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            }
        }

        private void AddBackground(RectTransform parent, Color bgColor)
        {
            var vPadding = 10f * borderScale;
            var hPadding = 18f * borderScale;
            GameObject go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(buttonWidth - hPadding, buttonHeight - vPadding);
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = bgColor;

            // Optionally store for later if you want to animate it
            _backgroundImage = img;
        }

        private void ClearChildren()
        {
#if UNITY_EDITOR
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
#else
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
#endif
        }
    }
}