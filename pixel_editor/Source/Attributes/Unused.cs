using System;
using System.Diagnostics.CodeAnalysis;

namespace pixel_editor.Source.Attributes
{
    public class Asdf
    {
        [Test("poopcicle")] public object asdf; 

    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple =false) ]
    public class TestAttribute : Attribute
    {
        public string name = "asd"; 
        public TestAttribute(string name)
        {
            this.name = name;
        }
    }
}