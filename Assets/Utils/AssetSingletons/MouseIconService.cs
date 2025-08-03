

using UnityEngine;

namespace UtilityClasses
{
    public static class MouseIconSingleton
    {
        public static Texture2D[] mouseTextures
        {
            get
            {
                return _mouseTextures;
            }
            set
            {
                if (_mouseTextures == null)
                {
                    _mouseTextures = value;
                }
                else
                {
                    Debug.LogException(new System.Exception("Error setting new mouse icons. Mouse icons are already configured."));
                }
            }
        }

        private static Texture2D[] _mouseTextures { get; set; }

        public static Texture2D GetMouseIcon(MouseAction mouseType)
        {
            return mouseTextures[(int)mouseType];
        }

        public static void SetCursorTexure(MouseAction mouseType)
        {
            switch (mouseType)
            {
                case MouseAction.None:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
                case MouseAction.Pointer:
                    Cursor.SetCursor(MouseIconSingleton.GetMouseIcon(MouseAction.Pointer), new Vector2(12, 0), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(GetMouseIcon(mouseType), Vector2.zero, CursorMode.Auto);
                    break;
            }
        }
    }
}