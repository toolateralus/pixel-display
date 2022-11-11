using System;
using System.Runtime.Serialization;

namespace pixel_renderer
{
    [Serializable]
    internal class MissingComponentException : Exception
    {
        public MissingComponentException()
        {

        }

        public MissingComponentException( Node node, Type typeOfComponent, 
            string message = "GetComponent called on a Node that does not have a component of provided type attached")
            : base(message)
        {
            
        }

        public MissingComponentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MissingComponentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}