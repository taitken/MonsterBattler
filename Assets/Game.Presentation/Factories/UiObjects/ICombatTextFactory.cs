using Game.Presentation.UI.Combat;
using UnityEngine;

namespace Assets.Game.Presentation.UiObjects
{
    public interface ICombatTextFactory
    {
        CombatTextUi Create(Color color, string text, Vector3 worldPosition);
    }
}