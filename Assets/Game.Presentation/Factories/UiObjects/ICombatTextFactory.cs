using UnityEngine;

namespace Assets.Game.Presentation.UiObjects
{
    public interface ICombatTextFactory
    {
        CombatTextUi Spawn(Color color, string text, Vector3 worldPosition);
    }
}