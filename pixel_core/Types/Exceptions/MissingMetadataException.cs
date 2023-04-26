using System;
using System.Runtime.Serialization;

namespace pixel_core
{
    [Serializable]
    internal class MissingMetadataException : Exception
    {
        public MissingMetadataException()
        {
        }
        public MissingMetadataException(string? message) : base(message)
        {
        }
        public MissingMetadataException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
        protected MissingMetadataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}