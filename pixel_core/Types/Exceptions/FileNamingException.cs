using System;
using System.Runtime.Serialization;

namespace pixel_core
{
    [Serializable]
    internal class FileNamingException : Exception
    {
        public FileNamingException()
        {
        }

        public FileNamingException(string? message) : base(message)
        {
        }

        public FileNamingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FileNamingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}