using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace pixel_editor.Source.Attributes
{
    public class Asdf
    {
        [Test] public object asdf; 

    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple =false) ]
    public class TestAttribute : Attribute
    {
        public TestAttribute()
        {
            
        }
    }
}