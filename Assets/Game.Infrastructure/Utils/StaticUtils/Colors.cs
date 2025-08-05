using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityClasses
{
    public class GameColors
    {
        public static Color AddTransparency(Color _oldColor, float _transparency)
        {
            return new Color(_oldColor.r, _oldColor.g, _oldColor.b, _transparency);
        }

        public static Color Lighten(Color _oldColor, float percentage)
        {
            return new Color(_oldColor.r * percentage, _oldColor.g * percentage, _oldColor.b * percentage, _oldColor.g);
        }
    }
}