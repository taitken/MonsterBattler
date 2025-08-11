using TMPro;
using UnityEngine;

namespace Game.Presentation.UI.ButtonUI
{
    public class ButtonLabel : MonoBehaviour
    {
        private TextMeshProUGUI _label;

        public void Awake()
        {
            _label = GetComponent<TextMeshProUGUI>();
        }

        public void SetText(string text, float fontSize = 28f)
        {
            if (_label != null)
            {
                _label.text = text;
                _label.fontSize = fontSize;
                _label.alignment = TextAlignmentOptions.Center;
                _label.raycastTarget = false;
                _label.fontStyle = FontStyles.SmallCaps;
                _label.textWrappingMode = TextWrappingModes.NoWrap;
            }
            else
            {
                Debug.LogWarning("ButtonLabel: Label is not assigned.");
            }
        }
    }
}

