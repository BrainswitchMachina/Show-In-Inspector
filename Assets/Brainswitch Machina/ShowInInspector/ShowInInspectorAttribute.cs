using System;


/// <summary>
/// To show Fields and Properties in the Unity Inspector, works on non-public and static fields and properties as well as public and instance fields and properties.
/// Note: Does not serialize anything.
/// Note: Static fields do not reset correctly when exiting Play mode in the Editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ShowInInspectorAttribute : Attribute
{
    public ShowInInspectorAttribute()
    {
    }
}