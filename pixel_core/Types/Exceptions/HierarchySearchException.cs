using System;
using System.Runtime.Serialization;

namespace pixel_core
{
    [Serializable]
    internal class HierarchySearchException : Exception
    {
        public HierarchySearchException()
        {
        }

        public HierarchySearchException(string? message) : base(message)
        {
        }

        public HierarchySearchException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected HierarchySearchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}