using Game.Presentation.UI.Combat;
using UnityEngine;

namespace Assets.Game.Presentation.UiObjects
{
    public interface ICombatTextFactory
    {
        CombatTextUi Create(Color color, string text, RectTransform rootCanvas, Vector3 worldPosition);
    }
}