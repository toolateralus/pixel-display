using System;
using System.Runtime.Serialization;

namespace pixel_renderer
{
    [Serializable]
    internal class EngineInstanceException : Exception
    {
        public EngineInstanceException()
        {
        }

        public EngineInstanceException(string? message) : base(message)
        {
        }

        public EngineInstanceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EngineInstanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}