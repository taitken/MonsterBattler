#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SystemTypeReference))]
public class SystemTypeReferenceDrawer : PropertyDrawer
{
    private const string k_AqNameProp = "assemblyQualifiedName";

    private List<Type> _types;                // cached list for this field
    private GUIContent[] _options;            // display names (first entry = (None))

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        // Find the attribute on this field to know the base type we filter by
        var attr = (SubclassOfAttribute)fieldInfo
            .GetCustomAttributes(typeof(SubclassOfAttribute), false)
            .FirstOrDefault();

        var baseType = attr?.BaseType ?? typeof(object);
        EnsureCache(baseType, includeAbstract: attr?.IncludeAbstract ?? false);

        var aqNameProp = prop.FindPropertyRelative(k_AqNameProp);
        var currentType = !string.IsNullOrEmpty(aqNameProp.stringValue)
            ? Type.GetType(aqNameProp.stringValue)
            : null;

        // Map current to index
        int index = 0; // 0 = (None)
        if (currentType != null)
        {
            for (int i = 0; i < _types.Count; i++)
            {
                if (_types[i] == currentType) { index = i + 1; break; }
            }
        }

        EditorGUI.BeginProperty(pos, label, prop);
        int newIndex = EditorGUI.Popup(pos, label, index, _options);
        if (newIndex != index)
        {
            aqNameProp.stringValue = (newIndex == 0) ? null : _types[newIndex - 1].AssemblyQualifiedName;
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;

    private void EnsureCache(Type baseType, bool includeAbstract)
    {
        if (_types != null) return;

        var derived = UnityEditor.TypeCache.GetTypesDerivedFrom(baseType);

        _types = derived
            .Where(t =>
                t != null &&
                !t.IsGenericTypeDefinition &&
                !t.IsInterface &&
                (includeAbstract || !t.IsAbstract) &&
                // allow value-types (structs) OR classes that have a parameterless ctor
                (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null))
            .OrderBy(NiceName)
            .ToList();

        var labels = new List<GUIContent> { new GUIContent("(None)") };
        labels.AddRange(_types.Select(t => new GUIContent(NiceName(t))));
        _options = labels.ToArray();
    }

    private static string NiceName(Type t)
    {
        // Show short name with namespace, handle nested types nicely
        return string.IsNullOrEmpty(t.Namespace) ? t.Name : $"{t.Namespace}.{t.Name}";
    }
}
#endif