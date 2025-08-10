#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Presentation.UI.Combat.Editor
{
    [CustomEditor(typeof(StretchableButtonFrame))]
    [CanEditMultipleObjects]
    public class StretchableButtonFrameEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild", GUILayout.Height(24)))
                {
                    foreach (var t in targets)
                    {
                        var frame = t as StretchableButtonFrame;
                        if (frame == null) continue;

                        Undo.RegisterFullObjectHierarchyUndo(frame.gameObject, "Rebuild Stretchable Button Frame");
                        frame.RebuildNow();
                        EditorUtility.SetDirty(frame);
                    }
                }
            }
        }
    }
}
#endif