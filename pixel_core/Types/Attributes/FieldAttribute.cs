﻿using System;

/// <summary>
/// An attribute for serializing fields to the InspectorControl.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class FieldAttribute : Attribute
{
    public FieldAttribute()
    {

    }
}
