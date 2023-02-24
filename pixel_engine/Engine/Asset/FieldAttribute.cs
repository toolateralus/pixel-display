using System;

namespace pixel_renderer
{
    /// <summary>
    /// An attribute for serializing fields to the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute() 
        {

        }
    }
}