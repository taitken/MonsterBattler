using Game.Presentation.UI.Combat;
using UnityEngine;

namespace Assets.Game.Presentation.UiObjects
{
    public class CombatTextFactory : ICombatTextFactory
    {
        private readonly GameObject _combatTextPrefab;
        public CombatTextFactory(GameObject prefab)
        {
            _combatTextPrefab = prefab;
        }
        public CombatTextUi Create(Color color, string text, RectTransform rootCanvas, Vector3 worldPosition)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas,
                screenPos,
                null, // ← Use `null` for Screen Space - Overlay!
                out Vector2 anchoredPos
            )) return null;

            anchoredPos += new Vector2(
                Random.Range(-80f, 80f), // pixels
                Random.Range(-15f, 15f)
            );

            var instance = Object.Instantiate(_combatTextPrefab, rootCanvas);
            var rectTransform = instance.GetComponent<RectTransform>();

            // ✅ Set center anchor and pivot for precise placement
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPos;

            var textUi = instance.GetComponent<CombatTextUi>();
            textUi.Show(text, color);

            return textUi;
        }
    }
}