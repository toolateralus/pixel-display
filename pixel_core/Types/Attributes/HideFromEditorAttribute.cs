using System;

namespace Pixel_Core.Types.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HideFromEditorAttribute : Attribute
    {
        /// <summary>
        /// This allows you to prevent a type from being added to AddComponent menu atm.
        /// </summary>
        public HideFromEditorAttribute()
        {
        }
    }
}