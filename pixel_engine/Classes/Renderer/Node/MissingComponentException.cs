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

        public MissingComponentException(string? message) : base(message)
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