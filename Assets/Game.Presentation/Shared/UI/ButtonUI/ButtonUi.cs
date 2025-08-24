using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Game.Application.Messaging;
using Game.Core;
using Game.Presentation.Models;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Presentation.UI.ButtonUI
{
    [AddComponentMenu("UI/Button UI (with events)")]
    [DisallowMultipleComponent]
    public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Sprites")]
        public Sprite cornerSprite;
        public Sprite horizontalSprite;
        public Sprite verticalSprite;
        public ButtonLabel gamelabelPrefab;

        [Header("Frame Settings")]
        public float cornerSize = 16f;
        [Range(1f, 3f)] public float borderScale = 2.0f;

        [Header("Target Dimensions")]
        public float buttonWidth = 200f;
        public float buttonHeight = 100f;

        [Header("Runtime Settings")]
        public string inputText = "Click Me";

        [Header("Button Config")]
        [SubclassOf(typeof(ICommand))]
        public CommandEntry[] messages;

        [Header("Hover Effects")]
        public Color hoverColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        [Range(1.0f, 1.3f)]
        public float hoverScale = 1.1f;
        [Range(0.05f, 0.5f)]
        public float hoverTransitionSpeed = 0.1f;

        // refs
        private ButtonLabel _currentLabel;
        private IEventBus _eventBus;
        private Image _backgroundImage;
        private RectTransform _rect;
        private Button _clickButton; // CHANGED: the actual button we click (child)

        private bool _needsRebuild;
        private Color _originalColor;
        private Vector3 _originalScale;
        private Coroutine _hoverTransition;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void Start()
        {
            // CHANGED: ensure a rebuild happens at play start after hierarchy is live
            DoRebuild();
            SetText(inputText);
            
            // Store original values for hover effects
            if (_backgroundImage != null)
            {
                _originalColor = _backgroundImage.color;
                _originalScale = transform.localScale;
            }
        }

        private void OnEnable()
        {
            RequestRebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!UnityEngine.Application.isPlaying)
                RequestRebuild();
        }
#endif

        private void OnRectTransformDimensionsChange()
        {
            if (!UnityEngine.Application.isPlaying)
                RequestRebuild();
        }

        [ContextMenu("Force Rebuild Frame")]
        public void RebuildNow() => DoRebuild();

        private void RequestRebuild()
        {
            if (!isActiveAndEnabled) return;
            _needsRebuild = true;

#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
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
            if (this == null || !isActiveAndEnabled) return;
            if (!UnityEngine.Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;
            if (_needsRebuild) DoRebuild();
        }
#endif

        private void DoRebuild()
        {
            _needsRebuild = false;

            _rect ??= GetComponent<RectTransform>();
            if (_rect == null) return;

            // CHANGED: only clear previous frame container, not all children (preserves label)
            ClearOldFrameOnly();

            var frameRoot = CreateFrameRoot();
            frameRoot.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            frameRoot.anchoredPosition = Vector2.zero;

            AddBackground(frameRoot, new Color(0.39f, 0.11f, 0.11f, 1f)); // sets _backgroundImage & _clickButton
            BuildFrame(frameRoot);
            EnsureLabel(frameRoot); // CHANGED: make sure label exists & is on top
            SnapChildrenToPixels(frameRoot);
            
            // Store original values for hover effects after background is created
            if (_backgroundImage != null)
            {
                _originalColor = _backgroundImage.color;
                _originalScale = transform.localScale;
            }
        }

        public void SetText(string text)
        {
            if (gamelabelPrefab == null)
            {
                Debug.LogWarning("ButtonUI: No label assigned.");
                return;
            }

            // scale against the visible background size if available
            float h = _backgroundImage ? ((RectTransform)_backgroundImage.transform).rect.height : _rect.rect.height;
            float w = _backgroundImage ? ((RectTransform)_backgroundImage.transform).rect.width : _rect.rect.width;

            float fontSizeByHeight = h * 0.30f;
            float fontSizeByWidth = w * 0.15f;
            var fontSize = Mathf.Min(fontSizeByHeight, fontSizeByWidth);
            gamelabelPrefab.SetText(text, fontSize);
        }

        private void OnClickFeedback()
        {
            if (!UnityEngine.Application.isPlaying) return;
            if (_backgroundImage != null)
            {
                StopAllCoroutines();
                StartCoroutine(FlashColor(_backgroundImage, hoverColor, 0.05f));
            }

            foreach (var message in messages)
            {
                Debug.Log("ButtonUI: Clicked!");
                var commandAsset = (ICommandAsset)message.commandAsset;
                if (commandAsset == null) return;
                Debug.Log($"ButtonUI: Publishing command {commandAsset.GetType().Name}");

                _eventBus?.Publish((dynamic)commandAsset.Create());
            }
        }

        private IEnumerator FlashColor(Image img, Color flashColor, float duration)
        {
            var original = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(duration);
            img.color = original;
        }

        private void BuildFrame(RectTransform frameRoot)
        {
            if (cornerSprite == null || horizontalSprite == null || verticalSprite == null)
                return;

            float w = buttonWidth;
            float h = buttonHeight;

            float cornerAspect = cornerSprite.rect.width / cornerSprite.rect.height;
            Vector2 cornerSizeVec = new Vector2(
                cornerSize * cornerAspect * borderScale,
                cornerSize * borderScale
            );

            float hThickness = GetHorizontalThickness() * borderScale;
            float vThickness = GetVerticalThickness() * borderScale;

            Vector2 topOffset = new Vector2(0, -3f) * borderScale;
            Vector2 bottomOffset = new Vector2(0, 3f) * borderScale;

            CreateImage("Top", horizontalSprite, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                Vector3.zero, frameRoot, topOffset);

            CreateImage("Bottom", horizontalSprite, new Vector2(0.5f, 0), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                new Vector3(180, 0, 0), frameRoot, bottomOffset);

            CreateImage("Left", verticalSprite, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                Vector3.zero, frameRoot);

            CreateImage("Right", verticalSprite, new Vector2(1, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                new Vector3(0, 180, 0), frameRoot);

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

        // --- helpers ---

        private RectTransform CreateFrameRoot()
        {
            var go = new GameObject("FrameRoot", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            return rt;
        }

        private void AddBackground(RectTransform parent, Color bgColor)
        {
            var vPadding = 10f * borderScale;
            var hPadding = 18f * borderScale;

            var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(buttonWidth - hPadding, buttonHeight - vPadding);
            rt.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;
            _backgroundImage = img;

            _clickButton = go.GetComponent<Button>();
            _clickButton.targetGraphic = img;

            // CHANGED: wire listener here, not in Awake
            _clickButton.onClick.RemoveAllListeners();
            _clickButton.onClick.AddListener(OnClickFeedback);
        }

        private void EnsureLabel(RectTransform parent)
        {
            // Reuse existing label if it still exists; otherwise create one
            if (_currentLabel == null)
            {
                _currentLabel = GetComponentInChildren<ButtonLabel>(includeInactive: true);
            }

            if (_currentLabel == null)
            {
                var go = Instantiate(gamelabelPrefab, parent, false);
                go.transform.SetParent(parent, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                _currentLabel = go;
            }
            else
            {
                // Ensure it’s under the new frame and centered
                var rt = (RectTransform)gamelabelPrefab.transform;
                rt.SetParent(parent, false);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }

            // keep label on top
            gamelabelPrefab.transform.SetAsLastSibling();
        }

        private void CreateImage(string name, Sprite sprite, Vector2 anchor, Vector2 pivot, Vector2 size, Vector3 rotation, Transform parent, Vector2? offset = null)
        {
            if (sprite == null) return;

            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
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

        private void SnapChildrenToPixels(Transform parent)
        {
            foreach (RectTransform rt in parent)
            {
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            }
        }

        private void ClearOldFrameOnly()
        {
#if UNITY_EDITOR
            // Only delete the previous frame container; keep other children (e.g., author-added label)
            var fr = transform.Find("FrameRoot");
            if (fr != null) DestroyImmediate(fr.gameObject);
#else
            var fr = transform.Find("FrameRoot");
            if (fr != null) Destroy(fr.gameObject);
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_backgroundImage == null) return;
            
            if (_hoverTransition != null)
                StopCoroutine(_hoverTransition);
                
            _hoverTransition = StartCoroutine(TransitionToHover(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_backgroundImage == null) return;
            
            if (_hoverTransition != null)
                StopCoroutine(_hoverTransition);
                
            _hoverTransition = StartCoroutine(TransitionToHover(false));
        }

        private IEnumerator TransitionToHover(bool isHovering)
        {
            Color targetColor = isHovering ? hoverColor : _originalColor;
            Vector3 targetScale = isHovering ? _originalScale * hoverScale : _originalScale;
            
            Color startColor = _backgroundImage.color;
            Vector3 startScale = transform.localScale;
            
            float elapsed = 0f;
            
            while (elapsed < hoverTransitionSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / hoverTransitionSpeed;
                t = Mathf.SmoothStep(0f, 1f, t);
                
                _backgroundImage.color = Color.Lerp(startColor, targetColor, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                yield return null;
            }
            
            _backgroundImage.color = targetColor;
            transform.localScale = targetScale;
            _hoverTransition = null;
        }
    }
}
