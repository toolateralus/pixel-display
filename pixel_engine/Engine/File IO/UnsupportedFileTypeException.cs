using System;
using System.Runtime.Serialization;

namespace pixel_renderer
{
    [Serializable]
    internal class UnsupportedFileTypeException : Exception
    {
        public UnsupportedFileTypeException()
        {
        }

        public UnsupportedFileTypeException(string? message) : base(message)
        {
        }

        public UnsupportedFileTypeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnsupportedFileTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}