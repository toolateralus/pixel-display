using System;

namespace Pixel_Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HideFromEditorAttribute : Attribute
    {
        public HideFromEditorAttribute()
        {
        }
    }
}