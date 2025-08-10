using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Presentation.UI.Combat
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class StretchableButtonFrame : MonoBehaviour
    {
        [Header("Sprites")]
        public Sprite cornerSprite;
        public Sprite horizontalSprite;
        public Sprite verticalSprite;

        [Header("Frame Settings")]
        public float cornerSize = 16f;
        [Range(1f, 3f)] public float borderScale = 2.0f;

        [Header("Target Dimensions")]
        public float buttonWidth = 200f;
        public float buttonHeight = 100f;

        [Header("Runtime Settings")]
        public string inputText = "Click Me";

        private Image _backgroundImage;
        private RectTransform _rect;
        private Button _button;
        private TextMeshProUGUI _label;

        // edit-mode safe rebuild flag
        private bool _needsRebuild;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _label = GetComponentInChildren<TextMeshProUGUI>();
            SetText(inputText);
            _button = GetComponent<Button>();
            if (UnityEngine.Application.isPlaying && _button != null)
                _button.onClick.AddListener(OnClickFeedback);
        }

        private void OnEnable()
        {
            _rect = _rect ?? GetComponent<RectTransform>();
            RequestRebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only schedule in edit mode; avoids recursive rebuild during property changes
            if (!UnityEngine.Application.isPlaying)
                RequestRebuild();
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            // If user scales the object in editor, rebuild layout
            if (!UnityEngine.Application.isPlaying)
                RequestRebuild();
        }

        [ContextMenu("Force Rebuild Frame")]
        public void RebuildNow()
        {
            DoRebuild();
        }

        private void RequestRebuild()
        {
            if (!isActiveAndEnabled) return;
            _needsRebuild = true;

#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                // Batch multiple inspector changes into one rebuild tick
                EditorApplication.delayCall -= TryDoDelayedRebuild;
                EditorApplication.delayCall += TryDoDelayedRebuild;
            }
            else
#endif
            {
                DoRebuild();
            }
        }

#if UNITY_EDITOR
        private void TryDoDelayedRebuild()
        {
            if (this == null) return; // component removed
            if (!isActiveAndEnabled) return;

            // Don’t run on prefab assets in the Project window
            if (!UnityEngine.Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(gameObject))
                return;

            if (_needsRebuild)
                DoRebuild();
        }
#endif

        private void DoRebuild()
        {
            _needsRebuild = false;

            _rect = _rect ?? GetComponent<RectTransform>();
            if (_rect == null) return;

            ClearChildren();

            // Create inner container sized to the button
            var frameRoot = GetOrCreateFrameRoot();
            frameRoot.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            frameRoot.anchoredPosition = Vector2.zero;

            AddBackground(frameRoot, new Color(0.849f, 0.849f, 0.849f, 1f)); // nice full color fill
            BuildFrame(frameRoot);
            SnapChildrenToPixels(frameRoot);
        }

        public void SetText(string text)
        {
            if (_label == null)
            {
                Debug.LogWarning("ButtonUI: No label assigned.");
                return;
            }

            _label.text = text;

            // Example scaling: font size is 40% of button height
            float fontSizeByHeight = _rect.rect.height * 0.3f;

            float fontSizeByWidth = _rect.rect.width * 0.15f;

            _label.fontSize = Mathf.Min(fontSizeByHeight, fontSizeByWidth);
        }

        private void OnClickFeedback()
        {
            Debug.Log("Button clicked: " + gameObject.name);
            if (!UnityEngine.Application.isPlaying) return; // no coroutines in edit mode
            if (_backgroundImage == null) return;

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

        private void BuildFrame(RectTransform frameRoot)
        {
            if (cornerSprite == null || horizontalSprite == null || verticalSprite == null)
                return;

            float w = buttonWidth;
            float h = buttonHeight;

            // Preserve corner aspect, scale by cornerSize and borderScale
            float cornerAspect = cornerSprite.rect.width / cornerSprite.rect.height;
            Vector2 cornerSizeVec = new Vector2(
                cornerSize * cornerAspect * borderScale,
                cornerSize * borderScale
            );

            // Thickness derived from sprite pixels, scaled to corner height
            float hThickness = GetHorizontalThickness() * borderScale;
            float vThickness = GetVerticalThickness() * borderScale;

            // Inward offsets for horizontal bars (unit space)
            Vector2 topOffset = new Vector2(0, -3f) * borderScale;
            Vector2 bottomOffset = new Vector2(0, 3f) * borderScale;

            // --- Draw order: sides first, corners last (corners on top) ---

            // Horizontal edges
            CreateImage("Top", horizontalSprite, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                Vector3.zero, frameRoot, topOffset);

            CreateImage("Bottom", horizontalSprite, new Vector2(0.5f, 0), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                new Vector3(180, 0, 0), frameRoot, bottomOffset);

            // Vertical edges
            CreateImage("Left", verticalSprite, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                Vector3.zero, frameRoot);

            CreateImage("Right", verticalSprite, new Vector2(1, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                new Vector3(0, 180, 0), frameRoot);

            // Corners (pivot corrections)
            CreateImage("TopLeft", cornerSprite, new Vector2(0, 1), new Vector2(0, 1),
                cornerSizeVec, Vector3.zero, frameRoot);

            CreateImage("TopRight", cornerSprite, new Vector2(1, 1), new Vector2(0, 1),
                cornerSizeVec, new Vector3(0, 180, 0), frameRoot);

            CreateImage("BottomLeft", cornerSprite, new Vector2(0, 0), new Vector2(0, 1),
                cornerSizeVec, new Vector3(180, 0, 0), frameRoot);

            CreateImage("BottomRight", cornerSprite, new Vector2(1, 0), new Vector2(0, 1),
                cornerSizeVec, new Vector3(180, 180, 0), frameRoot);
        }

        private float GetHorizontalThickness()
        {
            if (cornerSprite == null || horizontalSprite == null)
                return cornerSize;

            float cornerPx = cornerSprite.rect.height;
            float barPx = horizontalSprite.rect.height;
            return cornerSize * (barPx / Mathf.Max(1f, cornerPx));
        }

        private float GetVerticalThickness()
        {
            if (cornerSprite == null || verticalSprite == null)
                return cornerSize;

            float cornerPx = cornerSprite.rect.width;
            float barPx = verticalSprite.rect.width;
            return cornerSize * (barPx / Mathf.Max(1f, cornerPx));
        }

        private RectTransform GetOrCreateFrameRoot()
        {
            var existing = transform.Find("FrameRoot");
#if UNITY_EDITOR
            if (existing != null) DestroyImmediate(existing.gameObject);
#else
            if (existing != null) Destroy(existing.gameObject);
#endif
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
            img.raycastTarget = false;

            go.transform.localEulerAngles = rotation;
        }

        private void AddBackground(RectTransform parent, Color bgColor)
        {
            var vPadding = 10f * borderScale;
            var hPadding = 18f * borderScale;

            GameObject go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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
            img.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;                 // ensure button has a target graphic
            btn.onClick.RemoveListener(OnClickFeedback);
            btn.onClick.AddListener(OnClickFeedback); // hook up click here

            _backgroundImage = img;
        }

        private void SnapChildrenToPixels(Transform parent)
        {
            foreach (RectTransform rt in parent)
            {
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            }
        }

        private void ClearChildren()
        {
#if UNITY_EDITOR
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
#else
            foreach (Transform child in transform)
                Destroy(child.gameObject);
#endif
        }
    }
}