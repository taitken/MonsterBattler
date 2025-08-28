using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI.ButtonUI
{
    /// <summary>
    /// Handles building the visual frame elements for ButtonUI
    /// </summary>
    public class ButtonFrameBuilder
    {
        public Sprite CornerSprite { get; set; }
        public Sprite HorizontalSprite { get; set; }
        public Sprite VerticalSprite { get; set; }
        public float CornerSize { get; set; } = 16f;
        public float BorderScale { get; set; } = 2.0f;

        public void BuildFrame(RectTransform frameRoot, float buttonWidth, float buttonHeight)
        {
            if (CornerSprite == null || HorizontalSprite == null || VerticalSprite == null)
                return;

            float w = buttonWidth;
            float h = buttonHeight;

            float cornerAspect = CornerSprite.rect.width / CornerSprite.rect.height;
            Vector2 cornerSizeVec = new Vector2(
                CornerSize * cornerAspect * BorderScale,
                CornerSize * BorderScale
            );

            float hThickness = GetHorizontalThickness() * BorderScale;
            float vThickness = GetVerticalThickness() * BorderScale;

            Vector2 topOffset = new Vector2(0, -3f) * BorderScale;
            Vector2 bottomOffset = new Vector2(0, 3f) * BorderScale;

            CreateImage("Top", HorizontalSprite, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                Vector3.zero, frameRoot, topOffset);

            CreateImage("Bottom", HorizontalSprite, new Vector2(0.5f, 0), new Vector2(0.5f, 1),
                new Vector2(w - 2 * cornerSizeVec.x, hThickness),
                new Vector3(180, 0, 0), frameRoot, bottomOffset);

            CreateImage("Left", VerticalSprite, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                Vector3.zero, frameRoot);

            CreateImage("Right", VerticalSprite, new Vector2(1, 0.5f), new Vector2(0, 0.5f),
                new Vector2(vThickness, h - 2 * cornerSizeVec.y),
                new Vector3(0, 180, 0), frameRoot);

            CreateImage("TopLeft", CornerSprite, new Vector2(0, 1), new Vector2(0, 1),
                cornerSizeVec, Vector3.zero, frameRoot);

            CreateImage("TopRight", CornerSprite, new Vector2(1, 1), new Vector2(0, 1),
                cornerSizeVec, new Vector3(0, 180, 0), frameRoot);

            CreateImage("BottomLeft", CornerSprite, new Vector2(0, 0), new Vector2(0, 1),
                cornerSizeVec, new Vector3(180, 0, 0), frameRoot);

            CreateImage("BottomRight", CornerSprite, new Vector2(1, 0), new Vector2(0, 1),
                cornerSizeVec, new Vector3(180, 180, 0), frameRoot);
        }

        public void AddBackground(RectTransform parent, Color bgColor, float buttonWidth, float buttonHeight, out Image backgroundImage, out Button clickButton)
        {
            var vPadding = 10f * BorderScale;
            var hPadding = 18f * BorderScale;

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
            backgroundImage = img;

            clickButton = go.GetComponent<Button>();
            clickButton.targetGraphic = img;
        }

        public void SnapChildrenToPixels(Transform parent)
        {
            foreach (RectTransform rt in parent)
            {
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            }
        }

        private float GetHorizontalThickness()
        {
            if (CornerSprite == null || HorizontalSprite == null)
                return CornerSize;

            float cornerPx = CornerSprite.rect.height;
            float barPx = HorizontalSprite.rect.height;
            return CornerSize * (barPx / Mathf.Max(1f, cornerPx));
        }

        private float GetVerticalThickness()
        {
            if (CornerSprite == null || VerticalSprite == null)
                return CornerSize;

            float cornerPx = CornerSprite.rect.width;
            float barPx = VerticalSprite.rect.width;
            return CornerSize * (barPx / Mathf.Max(1f, cornerPx));
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
    }
}