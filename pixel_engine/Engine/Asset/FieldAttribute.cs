using System;

namespace pixel_renderer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class FieldAttribute : Attribute
    {
        public FieldAttribute() 
        {

        }
    }
}