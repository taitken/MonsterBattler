using UnityEngine;

public class CombatTextFactory : ICombatTextFactory
{
    private readonly GameObject _combatTextPrefab;
    private readonly RectTransform _combatTextContainer;
    public CombatTextFactory(GameObject prefab, RectTransform container)
    {
        _combatTextPrefab = prefab;
        _combatTextContainer = container;
    }
    public CombatTextUi Spawn(Color color, string text, Vector3 worldPosition)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _combatTextContainer,
            screenPos,
            null, // ← Use `null` for Screen Space - Overlay!
            out Vector2 anchoredPos
        )) return null;

        var instance = Object.Instantiate(_combatTextPrefab, _combatTextContainer);
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