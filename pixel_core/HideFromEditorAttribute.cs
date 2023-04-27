using System;

namespace pixel_editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HideFromEditorAttribute : Attribute
    {
        public HideFromEditorAttribute()
        {
        }
    }
}