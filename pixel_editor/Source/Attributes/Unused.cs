using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Pixel_Editor.Source.Attributes
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