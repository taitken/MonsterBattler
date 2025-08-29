using UnityEngine;

namespace Game.Presentation.Core.Helpers
{
    public static class ColorHelper
    {
        public static Color Lighten(Color color, float factor)
        {
            factor = Mathf.Clamp01(factor);
            
            return new Color(
                Mathf.Lerp(color.r, 1f, factor),
                Mathf.Lerp(color.g, 1f, factor),
                Mathf.Lerp(color.b, 1f, factor),
                color.a
            );
        }
        
        public static Color Darken(Color color, float factor)
        {
            factor = Mathf.Clamp01(factor);
            
            return new Color(
                Mathf.Lerp(color.r, 0f, factor),
                Mathf.Lerp(color.g, 0f, factor),
                Mathf.Lerp(color.b, 0f, factor),
                color.a
            );
        }
    }
}