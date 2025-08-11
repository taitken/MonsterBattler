using System;
using UnityEngine;

[Serializable]
public sealed class SystemTypeReference
{
    [SerializeField] private string assemblyQualifiedName;

    public Type Type
    {
        get => string.IsNullOrEmpty(assemblyQualifiedName) ? null : Type.GetType(assemblyQualifiedName);
        set => assemblyQualifiedName = value != null ? value.AssemblyQualifiedName : null;
    }

    public override string ToString() => Type != null ? Type.FullName : "(None)";
}