using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SubclassOfAttribute : PropertyAttribute
{
    public Type BaseType { get; }
    public bool IncludeAbstract { get; }

    public SubclassOfAttribute(Type baseType, bool includeAbstract = false)
    {
        BaseType = baseType ?? typeof(object);
        IncludeAbstract = includeAbstract;
    }
}