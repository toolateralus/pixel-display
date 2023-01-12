using System;
using System.Runtime.Serialization;

namespace pixel_editor
{
    [Serializable]
    internal class EditorEventNullException : NullReferenceException
    {
        public EditorEventNullException()
        {
        }

        public EditorEventNullException(string? message) : base(message)
        {
        }

        public EditorEventNullException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EditorEventNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}