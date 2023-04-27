using System;
using System.Runtime.Serialization;

namespace Pixel
{
    [Serializable]
    internal class NullStageException : NullReferenceException
    {
        public NullStageException()
        {
        }

        public NullStageException(string? message) : base(message + "Stage Not Found.")
        {


        }

        public NullStageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NullStageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}